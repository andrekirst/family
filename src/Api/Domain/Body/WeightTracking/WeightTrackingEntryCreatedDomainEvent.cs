using Api.Infrastructure;

namespace Api.Domain.Body.WeightTracking;

public record WeightTrackingEntryCreatedDomainEvent(
    int Id,
    DateTime MeasuredAt,
    WeightUnit WeightUnit,
    double Weight,
    int CreatedByFamilyMemberId,
    int CreatedForFamilyMemberId) : IDomainEvent, IDomainEventHasFamilyMemberInformation
{
    public string DomainEventName => "WeightTrackingEntryCreatedDomainEvent";
    public int DomainEventVersion => 1;
}