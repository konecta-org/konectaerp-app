using System.Text.Json;
using System.Linq;
using AuthenticationService.Messaging;
using AuthenticationService.Models;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedContracts.Events;

namespace AuthenticationService.BackgroundServices
{
    public class EmployeeCreatedConsumer : BackgroundService
    {
        private readonly IRabbitMqConnection _connection;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqOptions _options;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<EmployeeCreatedConsumer> _logger;
        private IModel? _channel;
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public EmployeeCreatedConsumer(
            IRabbitMqConnection connection,
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> options,
            IEventPublisher eventPublisher,
            ILogger<EmployeeCreatedConsumer> logger)
        {
            _connection = connection;
            _scopeFactory = scopeFactory;
            _eventPublisher = eventPublisher;
            _logger = logger;
            _options = options.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _channel = _connection.CreateChannel();
            _channel.ExchangeDeclare(exchange: _options.Exchange, type: ExchangeType.Topic, durable: true, autoDelete: false);
            _channel.QueueDeclare(queue: _options.EmployeeCreatedQueue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(_options.EmployeeCreatedQueue, _options.Exchange, _options.EmployeeCreatedRoutingKey);
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                await HandleMessageAsync(ea, stoppingToken);
            };

            _channel.BasicConsume(queue: _options.EmployeeCreatedQueue, autoAck: false, consumer: consumer);
            _logger.LogInformation("Started consuming HR employee created events on queue {Queue}", _options.EmployeeCreatedQueue);

            return Task.CompletedTask;
        }

        private async Task HandleMessageAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            if (_channel == null)
            {
                return;
            }

            var body = eventArgs.Body.ToArray();

            try
            {
                var payload = JsonSerializer.Deserialize<EmployeeCreatedEvent>(body, _serializerOptions);

                if (payload == null)
                {
                    _logger.LogWarning("Received null EmployeeCreatedEvent payload.");
                    _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                    return;
                }

                await ProvisionUserAsync(payload, cancellationToken);

                _channel.BasicAck(eventArgs.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process EmployeeCreatedEvent message.");
                _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
        }

        private async Task ProvisionUserAsync(EmployeeCreatedEvent employeeEvent, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

            var existingUser = await userManager.FindByEmailAsync(employeeEvent.WorkEmail);
            if (existingUser != null)
            {
                _logger.LogInformation("User with email {Email} already exists. Skipping provisioning.", employeeEvent.WorkEmail);
                return;
            }

            var password = PasswordGenerator.Generate();
            Console.WriteLine($"Generated password for {employeeEvent.WorkEmail}: {password}");
            var user = new ApplicationUser
            {
                UserName = employeeEvent.WorkEmail,
                Email = employeeEvent.WorkEmail,
                FullName = employeeEvent.FullName,
                EmployeeId = employeeEvent.EmployeeId,
                EmailConfirmed = false
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user for {Email}. Errors: {Errors}", employeeEvent.WorkEmail, errors);
                return;
            }

            _logger.LogInformation("Created identity user {UserId} for employee {EmployeeId}", user.Id, employeeEvent.EmployeeId);

            // try
            // {
            //     await emailSender.SendEmployeeCredentialsAsync(employeeEvent.PersonalEmail, employeeEvent.FullName, employeeEvent.WorkEmail, password);
            //     _logger.LogInformation("Sent credentials email to {PersonalEmail}", employeeEvent.PersonalEmail);
            // }
            // catch (Exception ex)
            // {
            //     _logger.LogError(ex, "Failed to send credentials email to {PersonalEmail}", employeeEvent.PersonalEmail);
            // }

            var roles = new[] { "Employee" };
            var userProvisionedEvent = new UserProvisionedEvent(
                user.Id,
                employeeEvent.EmployeeId,
                user.Email ?? string.Empty,
                user.FullName ?? string.Empty,
                roles,
                DateTime.UtcNow);

            await _eventPublisher.PublishAsync(_options.UserProvisionedRoutingKey, userProvisionedEvent, cancellationToken);
        }

        public override void Dispose()
        {
            base.Dispose();
            _channel?.Dispose();
        }
    }
}
