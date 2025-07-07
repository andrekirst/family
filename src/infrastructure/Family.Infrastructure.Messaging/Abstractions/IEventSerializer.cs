using Family.Infrastructure.EventSourcing.Models;

namespace Family.Infrastructure.Messaging.Abstractions;

public interface IEventSerializer
{
    string Serialize<T>(T @event) where T : DomainEvent;
    T? Deserialize<T>(string data) where T : DomainEvent;
    DomainEvent? Deserialize(string data, Type eventType);
    string GetEventTypeName<T>() where T : DomainEvent;
    Type? GetEventTypeFromName(string eventTypeName);
}