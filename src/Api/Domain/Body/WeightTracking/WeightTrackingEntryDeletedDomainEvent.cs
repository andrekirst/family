using Api.Infrastructure;

namespace Api.Domain.Body.WeightTracking;

public record WeightTrackingEntryDeletedDomainEvent(int Id, DateTime MeasuredAt, double Weight, WeightUnit WeightUnit) : IDomainEvent
{
    public string DomainEventName => nameof(WeightTrackingEntryDeletedDomainEvent);
    public int DomainEventVersion => 1;
}