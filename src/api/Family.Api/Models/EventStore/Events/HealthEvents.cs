namespace Family.Api.Models.EventStore.Events;

public record HealthAppointmentScheduledEvent : DomainEvent
{
    public required string AppointmentId { get; init; }
    public required string PatientId { get; init; }
    public required string PatientName { get; init; }
    public required string DoctorName { get; init; }
    public required string ClinicName { get; init; }
    public required DateTime AppointmentDateTime { get; init; }
    public required string AppointmentType { get; init; }
    public required string ScheduledBy { get; init; }
    public string? Notes { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}

public record MedicationPrescribedEvent : DomainEvent
{
    public required string PrescriptionId { get; init; }
    public required string PatientId { get; init; }
    public required string PatientName { get; init; }
    public required string MedicationName { get; init; }
    public required string Dosage { get; init; }
    public required string Frequency { get; init; }
    public required DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public required string PrescribedBy { get; init; }
    public string? Instructions { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}

public record VaccinationRecordedEvent : DomainEvent
{
    public required string VaccinationId { get; init; }
    public required string PatientId { get; init; }
    public required string PatientName { get; init; }
    public required string VaccineName { get; init; }
    public required DateTime VaccinationDate { get; init; }
    public required string AdministeredBy { get; init; }
    public required string Location { get; init; }
    public required string BatchNumber { get; init; }
    public required string RecordedBy { get; init; }
    public string? Notes { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}

public record HealthReminderSetEvent : DomainEvent
{
    public required string ReminderId { get; init; }
    public required string PatientId { get; init; }
    public required string PatientName { get; init; }
    public required string ReminderType { get; init; }
    public required string Title { get; init; }
    public required DateTime ReminderDateTime { get; init; }
    public required string SetBy { get; init; }
    public string? Notes { get; init; }
    public Dictionary<string, object> AdditionalProperties { get; init; } = new();
}