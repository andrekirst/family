using Family.Infrastructure.EventSourcing.Models;

namespace Family.Infrastructure.Messaging.Abstractions;

public interface IEventBusConsumer
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    
    void Subscribe<T>(string topic, Func<T, CancellationToken, Task> handler) 
        where T : DomainEvent;
    
    void Subscribe<T>(string topic, string consumerGroup, Func<T, CancellationToken, Task> handler) 
        where T : DomainEvent;
}