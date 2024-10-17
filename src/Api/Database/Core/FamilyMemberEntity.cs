using Api.Database.Body;
using Api.Domain.Core;
using Api.Infrastructure.Database;

namespace Api.Database.Core;

public class FamilyMemberEntity : BaseEntity, IHasLabelsEntity, IHasDomainEventEntities
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? Birthdate { get; set; }
    public ICollection<WeightTrackingEntity> WeightTrackingEntries { get; set; } = default!;
    public IEnumerable<LabelEntity> Labels { get; set; } = default!;
    public string? AspNetUserId { get; set; } = default!;
    public ICollection<DomainEventEntity> CreatedByDomainEventEntries { get; set; } = default!;
    public ICollection<DomainEventEntity> CreatedForDomainEventEntries { get; set; } = default!;
    public ICollection<Calendar.CalendarEntity> Calendars { get; set; } = default!;
    public ICollection<NotificationMessageEntity> NotificationMessages { get; set; } = default!;
}