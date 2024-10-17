using Api.Domain.Core;
using Api.Infrastructure;

namespace Api.Domain.Body.WeightTracking;

public class WeightTrackingEntity : BaseEntity
{
    public DateTime MeasuredAt { get; set; }
    public WeightUnit WeightUnit { get; set; } = WeightUnit.Kilogram;
    public double Weight { get; set; }
    public Guid FamilyMemberId { get; set; }
    public FamilyMemberEntity FamilyMemberEntity { get; set; } = default!;
}

public class WeightTrackingEntryMappings : IMap<WeightTrackingEntity, CreateWeightTrackingEntryCommandModel>
{
    public static WeightTrackingEntity MapFromSource(CreateWeightTrackingEntryCommandModel model) =>
        new WeightTrackingEntity
        {
            MeasuredAt = model.MeasuredAt,
            Weight = model.Weight,
            WeightUnit = model.WeightUnit
        };
}