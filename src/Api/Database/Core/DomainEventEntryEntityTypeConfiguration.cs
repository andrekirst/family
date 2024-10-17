using Api.Domain.Core;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Core;

public class DomainEventEntryEntityTypeConfiguration : EntityTypeConfigurationBase<DomainEventEntity>
{
    public override void Configure(EntityTypeBuilder<DomainEventEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("DomainEventEntries", SchemaNames.Core);

        builder
            .Property(p => p.EventType)
            .HasMaxLength(256)
            .IsUnicode(false);
    }
}