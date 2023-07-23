using Api.Domain.Core;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Core;

public class LabelEntityTypeConfiguration : EntityTypeConfigurationBase<Label>
{
    public override void Configure(EntityTypeBuilder<Label> builder)
    {
        base.Configure(builder);

        builder.ToTable("Labels", SchemaNames.Core);
        
        builder.ConfigureColor(_ => _.Color);

        builder
            .Property(p => p.Name)
            .HasMaxLength(265);
    }
}