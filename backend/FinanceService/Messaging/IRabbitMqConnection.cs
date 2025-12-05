using RabbitMQ.Client;

namespace FinanceService.Messaging
{
    public interface IRabbitMqConnection : IDisposable
    {
        IModel CreateChannel();
    }
}
