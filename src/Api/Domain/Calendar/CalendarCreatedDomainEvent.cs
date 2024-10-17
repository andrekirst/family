using Api.Infrastructure;

namespace Api.Domain.Calendar;

[DomainEvent("CalendarCreated", 1)]
public class CalendarCreatedDomainEvent : IDomainEvent, IDomainEventHasFamilyMemberInformation
{
    public string Name { get; init; } = default!;
    public Guid CreatedByFamilyMemberId { get; init; }
    public Guid CreatedForFamilyMemberId { get; init; }
}