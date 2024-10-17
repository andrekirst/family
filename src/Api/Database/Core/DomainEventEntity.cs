using Api.Infrastructure;

namespace Api.Database.Core;

public class DomainEventEntity : BaseEntity
{
    public string EventType { get; set; } = default!;
    public int EventVersion { get; set; } = default!;
    public string EventData { get; set; } = default!;
    public Guid? CreatedByFamilyMemberId { get; set; }
    public FamilyMemberEntity? CreatedByFamilyMember { get; set; } = default!;
    public Guid? CreatedForFamilyMemberId { get; set; }
    public FamilyMemberEntity? CreatedForFamilyMember { get; set; } = default!;
}