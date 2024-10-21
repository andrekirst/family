using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace Api.Infrastructure.DomainEvents;

public class DomainEventBus(IProducer<Null, string> producer) : IDomainEventBus
{
    public async Task<bool> SendAsync<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
        where TDomainEvent : IDomainEvent
    {
        var domainEventAttribute = domainEvent.GetDomainEventAttribute();
        var json = JsonSerializer.Serialize(domainEvent);

        var message = new Message<Null, string>
        {
            Value = json,
            Headers = new Headers
            {
                { DomainEventHeaderKeys.Name, Encoding.Default.GetBytes(domainEventAttribute.Name) },
                { DomainEventHeaderKeys.Version, Encoding.Default.GetBytes(domainEventAttribute.Version.ToString()) }
            }
        };
        
        var result = await producer.ProduceAsync("DomainEvents", message, cancellationToken);

        return result.Status == PersistenceStatus.Persisted;
    }
}