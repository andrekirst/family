namespace Api.Infrastructure.DomainEvents;

public interface IDomainEventBus
{
    Task<bool> SendAsync<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
        where TDomainEvent : IDomainEvent;
}