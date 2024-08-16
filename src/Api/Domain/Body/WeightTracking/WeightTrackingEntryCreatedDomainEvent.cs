using Api.Infrastructure;

namespace Api.Domain.Body.WeightTracking;

public record WeightTrackingEntryCreatedDomainEvent(
    Guid Id,
    DateTime MeasuredAt,
    WeightUnit WeightUnit,
    double Weight,
    Guid CreatedByFamilyMemberId,
    Guid CreatedForFamilyMemberId) : IDomainEvent, IDomainEventHasFamilyMemberInformation
{
    public string DomainEventName => "WeightTrackingEntryCreatedDomainEvent";
    public int DomainEventVersion => 1;
}