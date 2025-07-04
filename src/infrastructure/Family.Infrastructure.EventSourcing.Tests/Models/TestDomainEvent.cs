using Family.Infrastructure.EventSourcing.Models;

namespace Family.Infrastructure.EventSourcing.Tests.Models;

public record TestDomainEvent : DomainEvent
{
    public string? TestData { get; init; }
}