using Api.Domain.Body.WeightTracking;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Body;

public class WeightTrackingEntryEntityTypeConfiguration : EntityTypeConfigurationBase<WeightTrackingEntry>
{
    public override void Configure(EntityTypeBuilder<WeightTrackingEntry> builder)
    {
        base.Configure(builder);

        builder.ToTable("WeightTrackingEntries", SchemaNames.WeightTracking);
        builder.ConfigureEnumAsString(entry => entry.WeightUnit);
    }
}