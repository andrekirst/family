namespace Family.Api.Models.EventStore.Events;

public record UserLoginEvent : DomainEvent
{
    public required string UserId { get; init; }
    public required string UserName { get; init; }
    public required string UserEmail { get; init; }
    public required string IpAddress { get; init; }
    public required string UserAgent { get; init; }
    public required DateTime LoginTime { get; init; }
    public required bool IsSuccessful { get; init; }
    public string? FailureReason { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}

public record UserLogoutEvent : DomainEvent
{
    public required string UserId { get; init; }
    public required string UserName { get; init; }
    public required string UserEmail { get; init; }
    public required DateTime LogoutTime { get; init; }
    public required string LogoutReason { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}

public record SystemConfigurationChangedEvent : DomainEvent
{
    public required string ConfigurationKey { get; init; }
    public required string OldValue { get; init; }
    public required string NewValue { get; init; }
    public required string ChangedBy { get; init; }
    public required DateTime ChangeTime { get; init; }
    public string? Reason { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}

public record SystemErrorEvent : DomainEvent
{
    public required string ErrorId { get; init; }
    public required string ErrorType { get; init; }
    public required string ErrorMessage { get; init; }
    public required string StackTrace { get; init; }
    public required string Source { get; init; }
    public required DateTime ErrorTime { get; init; }
    public string? UserId { get; init; }
    public string? RequestId { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}

public record SystemBackupCompletedEvent : DomainEvent
{
    public required string BackupId { get; init; }
    public required string BackupType { get; init; }
    public required DateTime BackupStartTime { get; init; }
    public required DateTime BackupEndTime { get; init; }
    public required long BackupSize { get; init; }
    public required string BackupLocation { get; init; }
    public required bool IsSuccessful { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}