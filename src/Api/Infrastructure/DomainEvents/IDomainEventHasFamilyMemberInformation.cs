namespace Api.Infrastructure.DomainEvents;

public interface IDomainEventHasFamilyMemberInformation
{
    public Guid CreatedByFamilyMemberId { get; init; }
    public Guid CreatedForFamilyMemberId { get; init; }
}