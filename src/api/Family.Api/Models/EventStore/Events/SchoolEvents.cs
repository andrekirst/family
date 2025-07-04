namespace Family.Api.Models.EventStore.Events;

public record SchoolGradeEnteredEvent : DomainEvent
{
    public required string GradeId { get; init; }
    public required string StudentId { get; init; }
    public required string StudentName { get; init; }
    public required string Subject { get; init; }
    public required string Grade { get; init; }
    public required string GradeType { get; init; }
    public required DateTime GradeDate { get; init; }
    public required string EnteredBy { get; init; }
    public string? Notes { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}

public record SchoolHomeworkAssignedEvent : DomainEvent
{
    public required string HomeworkId { get; init; }
    public required string StudentId { get; init; }
    public required string StudentName { get; init; }
    public required string Subject { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required DateTime AssignedDate { get; init; }
    public required DateTime DueDate { get; init; }
    public required string AssignedBy { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}

public record SchoolParentMeetingScheduledEvent : DomainEvent
{
    public required string MeetingId { get; init; }
    public required string StudentId { get; init; }
    public required string StudentName { get; init; }
    public required string TeacherName { get; init; }
    public required DateTime MeetingDateTime { get; init; }
    public required string Location { get; init; }
    public required string Purpose { get; init; }
    public required string ScheduledBy { get; init; }
    public string? Notes { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}

public record SchoolMealOrderedEvent : DomainEvent
{
    public required string OrderId { get; init; }
    public required string StudentId { get; init; }
    public required string StudentName { get; init; }
    public required string MealType { get; init; }
    public required DateTime OrderDate { get; init; }
    public required DateTime MealDate { get; init; }
    public required decimal Cost { get; init; }
    public required string OrderedBy { get; init; }
    public string? SpecialRequests { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}