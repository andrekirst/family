using Api.Infrastructure;

namespace Api.Domain.Core;

public record FamilyMemberUpdatedDomainEvent(
    int Id,
    string FirstName,
    string LastName,
    DateTime Birthdate) : IDomainEvent
{
    public string DomainEventName => "FamilyMemberUpdatedDomainEvent";
    public int DomainEventVersion => 1;
}