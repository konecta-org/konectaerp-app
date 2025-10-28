using System.Text.Json;
using System.Linq;
using AuthenticationService.Messaging;
using AuthenticationService.Models;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedContracts.Events;

namespace AuthenticationService.BackgroundServices
{
    public class EmployeeEventsConsumer : BackgroundService
    {
        private readonly IRabbitMqConnection _connection;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly RabbitMqOptions _options;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<EmployeeEventsConsumer> _logger;
        private IModel? _channel;
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public EmployeeEventsConsumer(
            IRabbitMqConnection connection,
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> options,
            IEventPublisher eventPublisher,
            ILogger<EmployeeEventsConsumer> logger)
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
            _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true, autoDelete: false);
            _channel.QueueDeclare(queue: _options.EmployeeCreatedQueue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(_options.EmployeeCreatedQueue, _options.Exchange, _options.EmployeeCreatedRoutingKey);
            _channel.QueueBind(_options.EmployeeCreatedQueue, _options.Exchange, _options.EmployeeExitedRoutingKey);
            _channel.QueueBind(_options.EmployeeCreatedQueue, _options.Exchange, _options.EmployeeResignationApprovedRoutingKey);
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) => await HandleMessageAsync(ea, stoppingToken);

            _channel.BasicConsume(queue: _options.EmployeeCreatedQueue, autoAck: false, consumer: consumer);
            _logger.LogInformation("Listening for HR employee lifecycle events on queue {Queue}.", _options.EmployeeCreatedQueue);

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
                    case var key when key == _options.EmployeeCreatedRoutingKey:
                        await HandleEmployeeCreatedAsync(eventArgs, cancellationToken);
                        break;
                    case var key when key == _options.EmployeeExitedRoutingKey:
                        await HandleEmployeeExitedAsync(eventArgs, cancellationToken);
                        break;
                    case var key when key == _options.EmployeeResignationApprovedRoutingKey:
                        await HandleResignationApprovedAsync(eventArgs, cancellationToken);
                        break;
                    default:
                        _logger.LogWarning("Unhandled employee event routing key {RoutingKey}.", eventArgs.RoutingKey);
                        _channel.BasicAck(eventArgs.DeliveryTag, false);
                        return;
                }

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process employee event with routing key {RoutingKey}.", eventArgs.RoutingKey);
                _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
            }
        }

        private async Task HandleEmployeeCreatedAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Deserialize<EmployeeCreatedEvent>(eventArgs.Body.ToArray(), _serializerOptions);
            if (payload == null)
            {
                _logger.LogWarning("Received null EmployeeCreatedEvent payload.");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

            var existingUser = await userManager.FindByEmailAsync(payload.WorkEmail);
            if (existingUser != null)
            {
                _logger.LogInformation("User with email {Email} already exists. Skipping provisioning.", payload.WorkEmail);
                return;
            }

            var password = PasswordGenerator.Generate();
            var user = new ApplicationUser
            {
                UserName = payload.WorkEmail,
                Email = payload.WorkEmail,
                FullName = payload.FullName,
                EmployeeId = payload.EmployeeId,
                EmailConfirmed = false
            };

            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user for {Email}. Errors: {Errors}", payload.WorkEmail, errors);
                return;
            }

            _logger.LogInformation("Created identity user {UserId} for employee {EmployeeId}.", user.Id, payload.EmployeeId);

            try
            {
                await emailSender.SendEmployeeCredentialsAsync(payload.PersonalEmail, payload.FullName, payload.WorkEmail, password);
                _logger.LogInformation("Sent credentials email to {PersonalEmail}.", payload.PersonalEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send credentials email to {PersonalEmail}", payload.PersonalEmail);
            }

            var roles = new[] { "Employee" };
            var userProvisionedEvent = new UserProvisionedEvent(
                user.Id,
                payload.EmployeeId,
                user.Email ?? string.Empty,
                user.FullName ?? string.Empty,
                roles,
                DateTime.UtcNow);

            await _eventPublisher.PublishAsync(_options.UserProvisionedRoutingKey, userProvisionedEvent, cancellationToken);
        }

        private async Task HandleEmployeeExitedAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Deserialize<EmployeeExitedEvent>(eventArgs.Body.ToArray(), _serializerOptions);
            if (payload == null)
            {
                _logger.LogWarning("Received null EmployeeExitedEvent payload.");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user = null;

            if (!string.IsNullOrWhiteSpace(payload.UserId))
            {
                user = await userManager.FindByIdAsync(payload.UserId);
            }

            user ??= await userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == payload.EmployeeId);

            if (user == null)
            {
                _logger.LogWarning("Could not locate identity user for employee exit {EmployeeId}.", payload.EmployeeId);
                return;
            }

            if (!await userManager.GetLockoutEnabledAsync(user))
            {
                await userManager.SetLockoutEnabledAsync(user, true);
            }

            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            user.EmailConfirmed = false;

            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to deactivate user {UserId}. Errors: {Errors}", user.Id, errors);
                return;
            }

            var userDeactivatedEvent = new UserDeactivatedEvent(
                user.Id,
                payload.EmployeeId,
                DateTime.UtcNow,
                payload.Reason);

            await _eventPublisher.PublishAsync(_options.UserDeactivatedRoutingKey, userDeactivatedEvent, cancellationToken);
        }

        private async Task HandleResignationApprovedAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            var payload = JsonSerializer.Deserialize<EmployeeResignationApprovedEvent>(eventArgs.Body.ToArray(), _serializerOptions);
            if (payload == null)
            {
                _logger.LogWarning("Received null EmployeeResignationApprovedEvent payload.");
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user = null;

            if (payload.UserId.HasValue)
            {
                user = await userManager.FindByIdAsync(payload.UserId.Value.ToString());
            }

            user ??= await userManager.Users.FirstOrDefaultAsync(u => u.EmployeeId == payload.EmployeeId);

            if (user == null)
            {
                _logger.LogWarning("Could not locate identity user for resignation approval {EmployeeId}.", payload.EmployeeId);
            }
            else
            {
                var deleteResult = await userManager.DeleteAsync(user);
                if (!deleteResult.Succeeded)
                {
                    var errors = string.Join("; ", deleteResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to delete user {UserId} after resignation approval. Errors: {Errors}", user.Id, errors);
                    return;
                }

                _logger.LogInformation("Deleted identity user {UserId} after resignation approval.", user.Id);

                var userResignedEvent = new UserResignedEvent(
                    user.Id,
                    payload.EmployeeId,
                    payload.ResignationRequestId,
                    DateTime.UtcNow);

                await _eventPublisher.PublishAsync(_options.UserResignedRoutingKey, userResignedEvent, cancellationToken);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _channel?.Dispose();
        }
    }
}
