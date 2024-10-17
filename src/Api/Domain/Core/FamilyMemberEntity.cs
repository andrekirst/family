﻿using Api.Domain.Body.WeightTracking;
using Api.Domain.Core.Messaging;
using Api.Infrastructure;

namespace Api.Domain.Core;

public class FamilyMemberEntity : BaseEntity, IHasLabels, IHasDomainEventEntries
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