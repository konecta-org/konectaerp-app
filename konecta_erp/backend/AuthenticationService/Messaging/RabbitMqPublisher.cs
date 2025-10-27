using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AuthenticationService.Messaging
{
    public class RabbitMqPublisher : IEventPublisher
    {
        private readonly IRabbitMqConnection _connection;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqPublisher> _logger;

        public RabbitMqPublisher(IRabbitMqConnection connection, IOptions<RabbitMqOptions> options, ILogger<RabbitMqPublisher> logger)
        {
            _connection = connection;
            _logger = logger;
            _options = options.Value;
        }

        public Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default)
        {
            using var channel = _connection.CreateChannel();
            channel.ExchangeDeclare(exchange: _options.Exchange, type: ExchangeType.Topic, durable: true, autoDelete: false);

            var payload = JsonSerializer.SerializeToUtf8Bytes(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";

            channel.BasicPublish(
                exchange: _options.Exchange,
                routingKey: routingKey,
                body: payload,
                basicProperties: properties);

            _logger.LogInformation("Published event to {RoutingKey}", routingKey);

            return Task.CompletedTask;
        }
    }
}
