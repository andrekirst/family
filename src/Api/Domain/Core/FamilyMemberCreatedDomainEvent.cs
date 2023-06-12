using Api.Infrastructure;

namespace Api.Domain.Core;

public record FamilyMemberCreatedDomainEvent : IDomainEvent
{
    public FamilyMemberCreatedDomainEvent()
    {
    }

    public FamilyMemberCreatedDomainEvent(int id, string firstName, string lastName, DateTime birthdate)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Birthdate = birthdate;
    }

    public int Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime Birthdate { get; set; }
}