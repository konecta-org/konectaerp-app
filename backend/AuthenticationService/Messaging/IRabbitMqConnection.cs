using RabbitMQ.Client;

namespace AuthenticationService.Messaging
{
    public interface IRabbitMqConnection : IDisposable
    {
        IModel CreateChannel();
    }
}
