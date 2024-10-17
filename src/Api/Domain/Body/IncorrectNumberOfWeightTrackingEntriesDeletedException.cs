namespace Api.Domain.Body;

public class IncorrectNumberOfWeightTrackingEntriesDeletedException(int deletedRows, Guid id, Guid familyMemberId) : Exception
{
    public int DeletedRows { get; } = deletedRows;
    public Guid Id { get; } = id;
    public Guid FamilyMemberId { get; } = familyMemberId;
}