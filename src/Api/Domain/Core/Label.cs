using System.ComponentModel.DataAnnotations.Schema;
using Api.Infrastructure;

namespace Api.Domain.Core;

public class Label : BaseEntity
{
    public Color Color { get; set; } = new Color("#ffffff");
    public string Name { get; set; } = default!;
    public IEnumerable<FamilyMember> FamilyMembers { get; set; } = default!;
}