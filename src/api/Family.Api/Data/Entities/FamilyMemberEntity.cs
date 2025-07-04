using System.ComponentModel.DataAnnotations;
using FamilyMemberDomain = Family.Api.Features.Families.Models.FamilyMember;
using FamilyRole = Family.Api.Features.Families.Models.FamilyRole;

namespace Family.Api.Data.Entities;

public class FamilyMemberEntity
{
    [Required]
    [StringLength(255)]
    public string FamilyId { get; set; } = string.Empty;
    
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Role { get; set; } = string.Empty;
    
    [Required]
    public DateTime JoinedAt { get; set; }
    
    // Navigation property
    public FamilyEntity? Family { get; set; }
    
    // Factory method to create from domain model
    public static FamilyMemberEntity FromDomain(FamilyMemberDomain familyMember)
    {
        return new FamilyMemberEntity
        {
            FamilyId = familyMember.FamilyId,
            UserId = familyMember.UserId,
            Role = familyMember.Role.ToString(),
            JoinedAt = familyMember.JoinedAt
        };
    }
    
    // Method to convert to domain model
    public FamilyMemberDomain ToDomain()
    {
        var role = Enum.Parse<FamilyRole>(Role);
        return new FamilyMemberDomain(UserId, role, JoinedAt)
        {
            FamilyId = FamilyId
        };
    }
}