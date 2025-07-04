namespace Family.Api.Features.Families.Models;

public class FamilyMember
{
    public Guid UserId { get; set; }
    public FamilyRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    
    public FamilyMember(Guid userId, FamilyRole role, DateTime joinedAt)
    {
        UserId = userId;
        Role = role;
        JoinedAt = joinedAt;
    }
}

public enum FamilyRole
{
    FamilyUser = 0,
    FamilyAdmin = 1
}