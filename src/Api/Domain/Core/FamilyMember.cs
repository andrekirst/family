using Api.Domain.Body.WeightTracking;
using Api.Infrastructure;

namespace Api.Domain.Core;

public class FamilyMember : BaseEntity, IHasLabels, IHasDomainEventEntries
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? Birthdate { get; set; }
    public ICollection<WeightTrackingEntry> WeightTrackingEntries { get; set; } = default!;
    public IEnumerable<Label> Labels { get; set; } = default!;
    public string? AspNetUserId { get; set; } = default!;
    public ICollection<DomainEventEntry> CreatedByDomainEventEntries { get; set; } = default!;
    public ICollection<DomainEventEntry> CreatedForDomainEventEntries { get; set; } = default!;
}