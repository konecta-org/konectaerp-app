using RabbitMQ.Client;

namespace UserManagementService.Messaging
{
    public interface IRabbitMqConnection : IDisposable
    {
        IModel CreateChannel();
    }
}
