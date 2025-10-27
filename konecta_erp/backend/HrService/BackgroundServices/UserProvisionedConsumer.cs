using System.Text.Json;
using HrService.Messaging;
using HrService.Repositories;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedContracts.Events;

namespace HrService.BackgroundServices
{
    public class UserProvisionedConsumer : BackgroundService
    {
        private readonly IRabbitMqConnection _connection;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<UserProvisionedConsumer> _logger;
        private IModel? _channel;
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public UserProvisionedConsumer(
            IRabbitMqConnection connection,
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<UserProvisionedConsumer> logger)
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
            _channel.QueueDeclare(queue: _options.UserProvisionedQueue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(_options.UserProvisionedQueue, _options.Exchange, _options.UserProvisionedRoutingKey);
            _channel.BasicQos(0, 1, false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                await HandleMessageAsync(ea);
            };

            _channel.BasicConsume(queue: _options.UserProvisionedQueue, autoAck: false, consumer: consumer);
            _logger.LogInformation("Listening for identity provisioning events on queue {Queue}", _options.UserProvisionedQueue);

            return Task.CompletedTask;
        }

        private async Task HandleMessageAsync(BasicDeliverEventArgs eventArgs)
        {
            if (_channel == null)
            {
                return;
            }

            try
            {
                var payload = JsonSerializer.Deserialize<UserProvisionedEvent>(eventArgs.Body.ToArray(), _serializerOptions);
                if (payload == null)
                {
                    _logger.LogWarning("Received null UserProvisionedEvent payload.");
                    _channel.BasicAck(eventArgs.DeliveryTag, false);
                    return;
                }

                await ApplyIdentityToEmployeeAsync(payload);
                _channel.BasicAck(eventArgs.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process UserProvisionedEvent.");
                _channel?.BasicNack(eventArgs.DeliveryTag, false, requeue: false);
            }
        }

        private async Task ApplyIdentityToEmployeeAsync(UserProvisionedEvent provisionedEvent)
        {
            if (!Guid.TryParse(provisionedEvent.UserId, out var identityUserId))
            {
                _logger.LogWarning("Unable to parse identity user id '{UserId}' for employee {EmployeeId}", provisionedEvent.UserId, provisionedEvent.EmployeeId);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var employeeRepo = scope.ServiceProvider.GetRequiredService<IEmployeeRepo>();

            var updated = await employeeRepo.UpdateEmployeeIdentityAsync(provisionedEvent.EmployeeId, identityUserId);
            if (!updated)
            {
                _logger.LogWarning("Employee {EmployeeId} not found while applying identity id.", provisionedEvent.EmployeeId);
                return;
            }

            await employeeRepo.SaveChangesAsync();
            _logger.LogInformation("Linked employee {EmployeeId} to identity user {UserId}.", provisionedEvent.EmployeeId, provisionedEvent.UserId);
        }

        public override void Dispose()
        {
            base.Dispose();
            _channel?.Dispose();
        }
    }
}
