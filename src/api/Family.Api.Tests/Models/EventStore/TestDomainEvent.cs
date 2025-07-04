using Family.Api.Models.EventStore;

namespace Family.Api.Tests.Models.EventStore;

public record TestDomainEvent : DomainEvent
{
    public string? TestData { get; init; }
}