using Api.Infrastructure;

namespace Api.Domain.Core;

[DomainEvent("FamilyMemberUpdated")]
public record FamilyMemberUpdatedDomainEvent(
    int Id,
    string FirstName,
    string LastName,
    DateTime Birthdate) : IDomainEvent;