using Api.Infrastructure;

namespace Api.Domain.Core;

public class DomainEventEntry : BaseEntity
{
    public string EventType { get; set; } = default!;
    public int EventVersion { get; set; } = default!;
    public string EventData { get; set; } = default!;
    public int? CreatedByFamilyMemberId { get; set; }
    public FamilyMember? CreatedByFamilyMember { get; set; } = default!;
    public int? CreatedForFamilyMemberId { get; set; }
    public FamilyMember? CreatedForFamilyMember { get; set; } = default!;
}