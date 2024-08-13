using Api.Domain.Core;
using Api.Infrastructure;

namespace Api.Domain.Body.WeightTracking;

public class WeightTrackingEntry : BaseEntity
{
    public DateTime MeasuredAt { get; set; }
    public WeightUnit WeightUnit { get; set; } = WeightUnit.Kilogram;
    public double Weight { get; set; }
    public int FamilyMemberId { get; set; }
    public FamilyMember FamilyMember { get; set; } = default!;
}

public class WeightTrackingEntryMappings : IMap<WeightTrackingEntry, CreateWeightTrackingEntryCommandModel>
{
    public static WeightTrackingEntry MapFromSource(CreateWeightTrackingEntryCommandModel model) =>
        new WeightTrackingEntry
        {
            MeasuredAt = model.MeasuredAt,
            Weight = model.Weight,
            WeightUnit = model.WeightUnit
        };
}