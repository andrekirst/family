using Api.Infrastructure;

namespace Api.Domain.Body;

[DomainEvent("WeightTrackingEntryDeleted")]
public record WeightTrackingEntryDeletedDomainEvent(
    Guid Id,
    DateTime MeasuredAt,
    double Weight,
    WeightUnit WeightUnit) : IDomainEvent;