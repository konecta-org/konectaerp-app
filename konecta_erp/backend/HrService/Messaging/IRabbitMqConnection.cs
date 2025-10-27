using RabbitMQ.Client;

namespace HrService.Messaging
{
    public interface IRabbitMqConnection : IDisposable
    {
        IModel CreateChannel();
    }
}
