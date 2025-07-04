namespace Family.Api.Features.Families.DTOs;

public class FamilyDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public List<FamilyMemberDto> Members { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public static FamilyDto FromDomain(Models.Family family)
    {
        return new FamilyDto
        {
            Id = family.Id,
            Name = family.Name,
            OwnerId = family.OwnerId,
            Members = family.Members.Select(FamilyMemberDto.FromDomain).ToList(),
            CreatedAt = family.CreatedAt,
            UpdatedAt = family.UpdatedAt
        };
    }
}