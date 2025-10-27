using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace HrService.Messaging
{
    public class RabbitMqConnection : IRabbitMqConnection
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqConnection> _logger;
        private IConnection? _connection;
        private bool _disposed;

        public RabbitMqConnection(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnection> logger)
        {
            _logger = logger;
            _options = options.Value;

            _connectionFactory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                DispatchConsumersAsync = true
            };
        }

        public IModel CreateChannel()
        {
            EnsureConnected();
            return _connection!.CreateModel();
        }

        private void EnsureConnected()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(RabbitMqConnection));
            }

            if (_connection != null && _connection.IsOpen)
            {
                return;
            }

            try
            {
                _connection = _connectionFactory.CreateConnection();
                _connection.ConnectionShutdown += OnConnectionShutdown;
                _connection.CallbackException += OnCallbackException;
                _connection.ConnectionBlocked += OnConnectionBlocked;
                _logger.LogInformation("Connected to RabbitMQ at {Host}:{Port}", _options.HostName, _options.Port);
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.LogError(ex, "Could not reach RabbitMQ broker.");
                throw;
            }
            catch (SocketException ex)
            {
                _logger.LogError(ex, "Socket error while connecting to RabbitMQ.");
                throw;
            }
        }

        private void OnConnectionBlocked(object? sender, ConnectionBlockedEventArgs e)
        {
            _logger.LogWarning("RabbitMQ connection blocked: {Reason}", e.Reason);
        }

        private void OnCallbackException(object? sender, CallbackExceptionEventArgs e)
        {
            _logger.LogError(e.Exception, "RabbitMQ callback exception.");
        }

        private void OnConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            _logger.LogWarning("RabbitMQ connection shutdown: {ReplyText}", e.ReplyText);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;
            _connection?.Dispose();
        }
    }
}
