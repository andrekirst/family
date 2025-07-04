namespace Family.Api.Models.EventStore.Events;

public record FamilyCreatedEvent : DomainEvent
{
    public required string FamilyName { get; init; }
    public required string CreatedBy { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}

public record FamilyMemberAddedEvent : DomainEvent
{
    public required string MemberId { get; init; }
    public required string MemberName { get; init; }
    public required string MemberEmail { get; init; }
    public required string Role { get; init; }
    public required string AddedBy { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}

public record FamilyMemberRemovedEvent : DomainEvent
{
    public required string MemberId { get; init; }
    public required string MemberName { get; init; }
    public required string RemovedBy { get; init; }
    public string? Reason { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}

public record FamilyMemberRoleChangedEvent : DomainEvent
{
    public required string MemberId { get; init; }
    public required string MemberName { get; init; }
    public required string OldRole { get; init; }
    public required string NewRole { get; init; }
    public required string ChangedBy { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}