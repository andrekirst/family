using Api.Infrastructure;

namespace Api.Domain.Body;

[DomainEvent("WeightTrackingEntryUpdatedDomainEvent")]
public record WeightTrackingEntryUpdatedDomainEvent(
    Guid Id,
    DateTime MeasuredAt,
    double Weight,
    WeightUnit WeightUnit) : IDomainEvent;