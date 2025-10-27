namespace HrService.Messaging
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default);
    }
}
