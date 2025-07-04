using Family.Api.Features.Families.Models;

namespace Family.Api.Features.Families.DTOs;

public class FamilyMemberDto
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }

    public static FamilyMemberDto FromDomain(FamilyMember member)
    {
        return new FamilyMemberDto
        {
            UserId = member.UserId,
            Role = member.Role.ToString(),
            JoinedAt = member.JoinedAt
        };
    }
}