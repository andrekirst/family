namespace Api.Infrastructure;

public interface IDomainEventHasFamilyMemberInformation
{
    public int CreatedByFamilyMemberId { get; init; }
    public int CreatedForFamilyMemberId { get; init; }
}