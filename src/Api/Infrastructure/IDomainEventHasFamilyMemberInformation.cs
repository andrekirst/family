namespace Api.Infrastructure;

public interface IDomainEventHasFamilyMemberInformation
{
    public Guid CreatedByFamilyMemberId { get; init; }
    public Guid CreatedForFamilyMemberId { get; init; }
}