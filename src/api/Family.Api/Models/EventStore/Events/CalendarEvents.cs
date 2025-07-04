namespace Family.Api.Models.EventStore.Events;

public record CalendarAppointmentCreatedEvent : DomainEvent
{
    public required string AppointmentId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required DateTime StartTime { get; init; }
    public required DateTime EndTime { get; init; }
    public required string Location { get; init; }
    public required string[] Attendees { get; init; }
    public required string CreatedBy { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}

public record CalendarAppointmentUpdatedEvent : DomainEvent
{
    public required string AppointmentId { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required DateTime StartTime { get; init; }
    public required DateTime EndTime { get; init; }
    public required string Location { get; init; }
    public required string[] Attendees { get; init; }
    public required string UpdatedBy { get; init; }
    public Dictionary<string, object> Changes { get; init; } = new();
}

public record CalendarAppointmentDeletedEvent : DomainEvent
{
    public required string AppointmentId { get; init; }
    public required string Title { get; init; }
    public required string DeletedBy { get; init; }
    public string? Reason { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}

public record CalendarAppointmentReminderSetEvent : DomainEvent
{
    public required string AppointmentId { get; init; }
    public required string Title { get; init; }
    public required TimeSpan ReminderOffset { get; init; }
    public required string[] Recipients { get; init; }
    public required string SetBy { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}