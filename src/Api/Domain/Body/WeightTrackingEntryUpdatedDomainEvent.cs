using Api.Infrastructure.DomainEvents;

namespace Api.Domain.Body;

[DomainEvent("WeightTrackingEntryUpdatedDomainEvent")]
public record WeightTrackingEntryUpdatedDomainEvent(
    Guid Id,
    DateTime MeasuredAt,
    double Weight,
    WeightUnit WeightUnit) : IDomainEvent;