using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedContracts.Events;
using UserManagementService.Dtos;
using UserManagementService.Messaging;
using UserManagementService.Services;

namespace UserManagementService.BackgroundServices;

public class UserEventsConsumer : BackgroundService
{
    private readonly IRabbitMqConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<UserEventsConsumer> _logger;
    private IModel? _channel;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public UserEventsConsumer(
        IRabbitMqConnection connection,
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<UserEventsConsumer> logger)
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
        _channel.QueueDeclare(_options.UserEventsQueue, durable: true, exclusive: false, autoDelete: false);

        BindQueue(_options.UserProvisionedRoutingKey);
        BindQueue(_options.UserDeactivatedRoutingKey);
        BindQueue(_options.UserResignedRoutingKey);
        BindQueue(_options.UserTerminatedRoutingKey);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) => await HandleMessageAsync(ea, stoppingToken);

        _channel.BasicQos(0, 1, false);
        _channel.BasicConsume(queue: _options.UserEventsQueue, autoAck: false, consumer: consumer);

        _logger.LogInformation("Listening for user lifecycle events on queue {Queue}.", _options.UserEventsQueue);
        return Task.CompletedTask;
    }

    private void BindQueue(string routingKey)
    {
        if (string.IsNullOrWhiteSpace(routingKey) || _channel == null)
        {
            return;
        }

        _channel.QueueBind(_options.UserEventsQueue, _options.Exchange, routingKey);
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
                case var key when key == _options.UserProvisionedRoutingKey:
                    await HandleUserProvisionedAsync(eventArgs, cancellationToken);
                    break;
                case var key when key == _options.UserDeactivatedRoutingKey:
                    await HandleUserDeactivatedAsync(eventArgs, cancellationToken);
                    break;
                case var key when key == _options.UserResignedRoutingKey:
                    await HandleUserResignedAsync(eventArgs, cancellationToken);
                    break;
                case var key when key == _options.UserTerminatedRoutingKey:
                    await HandleUserTerminatedAsync(eventArgs, cancellationToken);
                    break;
                default:
                    _logger.LogWarning("Unhandled user event routing key {RoutingKey}.", eventArgs.RoutingKey);
                    _channel.BasicAck(eventArgs.DeliveryTag, false);
                    return;
            }

            _channel.BasicAck(eventArgs.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process user event with routing key {RoutingKey}.", eventArgs.RoutingKey);
            _channel.BasicNack(eventArgs.DeliveryTag, multiple: false, requeue: false);
        }
    }

    private async Task HandleUserProvisionedAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<UserProvisionedEvent>(eventArgs.Body.ToArray(), _serializerOptions);
        if (payload == null)
        {
            _logger.LogWarning("Received null UserProvisionedEvent payload.");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var primaryRole = payload.Roles?.FirstOrDefault() ?? "Employee";

        await userService.CreateOrUpdateFromExternalAsync(
            payload.UserId,
            payload.WorkEmail,
            payload.FullName,
            primaryRole,
            cancellationToken);

        _logger.LogInformation("Upserted user {UserId} from provisioned event.", payload.UserId);
    }

    private async Task HandleUserDeactivatedAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<UserDeactivatedEvent>(eventArgs.Body.ToArray(), _serializerOptions);
        if (payload == null)
        {
            _logger.LogWarning("Received null UserDeactivatedEvent payload.");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var dto = new UserStatusUpdateDto
        {
            Status = "Inactive",
            LockAccount = true,
            ChangedBy = "System"
        };

        var updated = await userService.UpdateStatusAsync(payload.UserId, dto, cancellationToken);
        if (updated)
        {
            _logger.LogInformation("Marked user {UserId} inactive from deactivation event.", payload.UserId);
        }
        else
        {
            _logger.LogWarning("User {UserId} not found while processing deactivation event.", payload.UserId);
        }
    }

    private async Task HandleUserResignedAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<UserResignedEvent>(eventArgs.Body.ToArray(), _serializerOptions);
        if (payload == null)
        {
            _logger.LogWarning("Received null UserResignedEvent payload.");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var deleted = await userService.SoftDeleteAsync(payload.UserId, cancellationToken);
        if (deleted)
        {
            _logger.LogInformation("Soft-deleted user {UserId} due to resignation event.", payload.UserId);
        }
        else
        {
            _logger.LogWarning("User {UserId} not found while processing resignation event.", payload.UserId);
        }
    }

    private async Task HandleUserTerminatedAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Deserialize<UserTerminatedEvent>(eventArgs.Body.ToArray(), _serializerOptions);
        if (payload == null)
        {
            _logger.LogWarning("Received null UserTerminatedEvent payload.");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var terminated = await userService.TerminateAsync(payload.UserId, payload.Reason, cancellationToken);
        if (terminated)
        {
            _logger.LogInformation("Terminated user {UserId} from termination event.", payload.UserId);
        }
        else
        {
            _logger.LogWarning("User {UserId} not found while processing termination event.", payload.UserId);
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        _channel?.Dispose();
    }
}
