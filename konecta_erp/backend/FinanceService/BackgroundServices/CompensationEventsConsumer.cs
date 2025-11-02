using System.Linq;
using System.Text.Json;
using FinanceService.Dtos;
using FinanceService.Messaging;
using FinanceService.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedContracts.Events;

namespace FinanceService.BackgroundServices
{
    public class CompensationEventsConsumer : BackgroundService
    {
        private readonly IRabbitMqConnection _connection;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<CompensationEventsConsumer> _logger;
        private IModel? _channel;
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public CompensationEventsConsumer(
            IRabbitMqConnection connection,
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<CompensationEventsConsumer> logger)
        {
            _connection = connection;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _channel = _connection.CreateChannel();
            _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true, autoDelete: false);
            _channel.QueueDeclare(queue: _options.CompensationEventsQueue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(_options.CompensationEventsQueue, _options.Exchange, _options.CompensationProvisionedRoutingKey);
            _channel.QueueBind(_options.CompensationEventsQueue, _options.Exchange, _options.CompensationBonusesRoutingKey);
            _channel.QueueBind(_options.CompensationEventsQueue, _options.Exchange, _options.CompensationDeductionsRoutingKey);
            _channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                await HandleMessageAsync(ea, stoppingToken);
            };

            _channel.BasicConsume(queue: _options.CompensationEventsQueue, autoAck: false, consumer: consumer);
            _logger.LogInformation("Finance service listening for compensation events on queue {Queue}", _options.CompensationEventsQueue);

            return Task.CompletedTask;
        }

        private async Task HandleMessageAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            if (_channel == null)
            {
                return;
            }

            try
            {
                switch (eventArgs.RoutingKey)
                {
                    case var key when key == _options.CompensationProvisionedRoutingKey:
                        await HandleProvisionedAsync(eventArgs, cancellationToken);
                        break;
                    case var key when key == _options.CompensationBonusesRoutingKey:
                        await HandleBonusesAsync(eventArgs, cancellationToken);
                        break;
                    case var key when key == _options.CompensationDeductionsRoutingKey:
                        await HandleDeductionsAsync(eventArgs, cancellationToken);
                        break;
                    default:
                        _logger.LogWarning("Unhandled compensation event routing key {RoutingKey}", eventArgs.RoutingKey);
                        break;
                }

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process compensation event with routing key {RoutingKey}", eventArgs.RoutingKey);
                _channel.BasicNack(eventArgs.DeliveryTag, false, requeue: false);
            }
        }

        private async Task HandleProvisionedAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Deserialize<EmployeeCompensationProvisionedEvent>(eventArgs.Body.ToArray(), _serializerOptions);
            if (payload == null)
            {
                _logger.LogWarning("Received null EmployeeCompensationProvisionedEvent payload.");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var compensationService = scope.ServiceProvider.GetRequiredService<IEmployeeCompensationService>();

            var upsertDto = new EmployeeAccountUpsertDto
            {
                EmployeeId = payload.EmployeeId.ToString(),
                EmployeeName = payload.FullName,
                Department = payload.DepartmentName,
                JobTitle = payload.Position,
                BaseSalary = Math.Round(payload.BaseSalary, 2, MidpointRounding.AwayFromZero),
                Currency = payload.Currency,
                EffectiveFrom = payload.EffectiveFrom
            };

            await compensationService.UpsertAccountAsync(upsertDto, cancellationToken);
            _logger.LogInformation("Provisioned/updated compensation account for employee {EmployeeId}", payload.EmployeeId);
        }

        private async Task HandleBonusesAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Deserialize<EmployeeCompensationBonusesIssuedEvent>(eventArgs.Body.ToArray(), _serializerOptions);
            if (payload == null)
            {
                _logger.LogWarning("Received null EmployeeCompensationBonusesIssuedEvent payload.");
                return;
            }

            if (payload.Bonuses == null || payload.Bonuses.Count == 0)
            {
                _logger.LogInformation("No bonus entries present for employee {EmployeeId}.", payload.EmployeeId);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var compensationService = scope.ServiceProvider.GetRequiredService<IEmployeeCompensationService>();

            var bonusDtos = payload.Bonuses.Select(b => new CompensationBonusCreateDto
            {
                BonusType = b.BonusType,
                Amount = Math.Round(b.Amount, 2, MidpointRounding.AwayFromZero),
                AwardedOn = b.AwardedOn,
                Period = b.Period,
                Reference = b.Reference,
                AwardedBy = b.AwardedBy,
                Notes = b.Notes,
                SourceSystem = b.SourceSystem
            });

            await compensationService.AddBonusesAsync(payload.EmployeeId.ToString(), bonusDtos, cancellationToken);
            _logger.LogInformation("Processed {BonusCount} bonuses for employee {EmployeeId}", payload.Bonuses.Count, payload.EmployeeId);
        }

        private async Task HandleDeductionsAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Deserialize<EmployeeCompensationDeductionsIssuedEvent>(eventArgs.Body.ToArray(), _serializerOptions);
            if (payload == null)
            {
                _logger.LogWarning("Received null EmployeeCompensationDeductionsIssuedEvent payload.");
                return;
            }

            if (payload.Deductions == null || payload.Deductions.Count == 0)
            {
                _logger.LogInformation("No deduction entries present for employee {EmployeeId}.", payload.EmployeeId);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var compensationService = scope.ServiceProvider.GetRequiredService<IEmployeeCompensationService>();

            var deductionDtos = payload.Deductions.Select(d => new CompensationDeductionCreateDto
            {
                DeductionType = d.DeductionType,
                Amount = Math.Round(d.Amount, 2, MidpointRounding.AwayFromZero),
                AppliedOn = d.AppliedOn,
                Period = d.Period,
                Reference = d.Reference,
                AppliedBy = d.AppliedBy,
                Notes = d.Notes,
                SourceSystem = d.SourceSystem,
                IsRecurring = d.IsRecurring
            });

            await compensationService.AddDeductionsAsync(payload.EmployeeId.ToString(), deductionDtos, cancellationToken);
            _logger.LogInformation("Processed {DeductionCount} deductions for employee {EmployeeId}", payload.Deductions.Count, payload.EmployeeId);
        }

        public override void Dispose()
        {
            base.Dispose();
            _channel?.Dispose();
        }
    }
}
