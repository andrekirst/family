using Api.Database.Core;
using Api.Infrastructure;

namespace Api.Domain.Core;

public class LabelEntity : BaseEntity
{
    public Color Color { get; set; } = new Color("#ffffff");
    public string Name { get; set; } = default!;
    public IEnumerable<FamilyMemberEntity> FamilyMembers { get; set; } = default!;
}