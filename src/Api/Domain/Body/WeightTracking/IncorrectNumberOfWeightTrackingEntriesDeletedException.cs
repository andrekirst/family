namespace Api.Domain.Body.WeightTracking;

public class IncorrectNumberOfWeightTrackingEntriesDeletedException : Exception
{
    public int DeletedRows { get; }
    public int Id { get; }
    public int FamilyMemberId { get; }

    public IncorrectNumberOfWeightTrackingEntriesDeletedException(int deletedRows, int id, int familyMemberId)
    {
        DeletedRows = deletedRows;
        Id = id;
        FamilyMemberId = familyMemberId;
    }
}