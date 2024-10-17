using Api.Infrastructure;

namespace Api.Domain.Body;

[DomainEvent("WeightTrackingEntryCreated")]
public record WeightTrackingEntryCreatedDomainEvent(
    Guid Id,
    DateTime MeasuredAt,
    WeightUnit WeightUnit,
    double Weight,
    Guid CreatedByFamilyMemberId,
    Guid CreatedForFamilyMemberId) : IDomainEvent, IDomainEventHasFamilyMemberInformation;