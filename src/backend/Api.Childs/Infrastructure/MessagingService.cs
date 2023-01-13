namespace Api.Childs.Infrastructure;

public interface IMessagingService
{
    Task SendEventAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default);
}

public class MessagingService : IMessagingService
{
    public Task SendEventAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}