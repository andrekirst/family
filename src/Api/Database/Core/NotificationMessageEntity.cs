using Api.Infrastructure.Database;

namespace Api.Database.Core;

public class NotificationMessageEntity : BaseEntity
{
    public bool MarkedAsRead { get; set; }
    public Guid FamilyMemberId { get; set; }
    public FamilyMemberEntity FamilyMemberEntity { get; set; } = default!;
    public string MessageType { get; set; } = default!;
    public string Version { get; set; } = default!;
    public string Instance { get; set; } = default!;
}