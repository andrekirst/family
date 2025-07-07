using Family.Infrastructure.EventSourcing.Models;

namespace Family.Infrastructure.Messaging.Abstractions;

public interface IEventBusPublisher
{
    Task PublishAsync<T>(T @event, string? topic = null, CancellationToken cancellationToken = default) 
        where T : DomainEvent;
    
    Task PublishAsync<T>(T @event, string topic, string? key = null, CancellationToken cancellationToken = default) 
        where T : DomainEvent;
    
    Task PublishBatchAsync<T>(IEnumerable<T> events, string? topic = null, CancellationToken cancellationToken = default) 
        where T : DomainEvent;
}