namespace WebUi.ValueObjects;

public record FamilyMemberId
{
    public int Id { get; }

    public FamilyMemberId(int id)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(id, 0, nameof(id));
        Id = id;
    }

    public static FamilyMemberId FromValue(int id) => new FamilyMemberId(id);
}