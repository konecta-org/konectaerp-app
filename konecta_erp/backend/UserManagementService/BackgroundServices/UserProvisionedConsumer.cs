using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedContracts.Events;
using UserManagementService.Data;
using UserManagementService.Messaging;
using UserManagementService.Models;

namespace UserManagementService.BackgroundServices
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
            consumer.Received += async (_, ea) => await HandleMessageAsync(ea);

            _channel.BasicConsume(queue: _options.UserProvisionedQueue, autoAck: false, consumer: consumer);
            _logger.LogInformation("Listening for user provisioned events on queue {Queue}", _options.UserProvisionedQueue);

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

                await UpsertUserAsync(payload);
                _channel.BasicAck(eventArgs.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process UserProvisionedEvent.");
                _channel?.BasicNack(eventArgs.DeliveryTag, false, requeue: false);
            }
        }

        private async Task UpsertUserAsync(UserProvisionedEvent provisionedEvent)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();

            var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.WorkEmail == provisionedEvent.WorkEmail);
            if (existingUser == null)
            {
                var newUser = new KonectaUser
                {
                    Id = Guid.TryParse(provisionedEvent.UserId, out var parsedId) ? parsedId : Guid.NewGuid(),
                    EmployeeId = provisionedEvent.EmployeeId,
                    WorkEmail = provisionedEvent.WorkEmail,
                    FullName = provisionedEvent.FullName,
                    Roles = string.Join(',', provisionedEvent.Roles),
                    ProvisionedAt = provisionedEvent.ProvisionedAt
                };

                dbContext.Users.Add(newUser);
                _logger.LogInformation("Created user management record for {WorkEmail}.", provisionedEvent.WorkEmail);
            }
            else
            {
                existingUser.FullName = provisionedEvent.FullName;
                existingUser.Roles = string.Join(',', provisionedEvent.Roles);
                existingUser.ProvisionedAt = provisionedEvent.ProvisionedAt;
                existingUser.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Updated user management record for {WorkEmail}.", provisionedEvent.WorkEmail);
            }

            await dbContext.SaveChangesAsync();
        }

        public override void Dispose()
        {
            base.Dispose();
            _channel?.Dispose();
        }
    }
}
