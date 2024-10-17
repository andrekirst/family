using Api.Infrastructure;

namespace Api.Domain.Body;

public record WeightTrackingEntryUpdatedDomainEvent(
    Guid Id,
    DateTime MeasuredAt,
    double Weight,
    WeightUnit WeightUnit) : IDomainEvent
{
    public string DomainEventName => "WeightTrackingEntryUpdatedDomainEvent";
    public int DomainEventVersion => 1;
}