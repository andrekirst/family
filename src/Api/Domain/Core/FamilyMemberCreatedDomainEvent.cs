using Api.Infrastructure;

namespace Api.Domain.Core;

public record FamilyMemberCreatedDomainEvent(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime Birthdate,
    string? AspNetUserId) : IDomainEvent
{
    public string DomainEventName => "FamilyMemberCreatedDomainEvent";
    public int DomainEventVersion => 1;
}