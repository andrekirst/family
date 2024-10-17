﻿using Api.Domain.Core;
using Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Core;

public class LabelEntityTypeConfiguration : EntityTypeConfigurationBase<LabelEntity>
{
    public override void Configure(EntityTypeBuilder<LabelEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("Labels", SchemaNames.Core);
        
        builder.ConfigureColor(e => e.Color);

        builder
            .Property(p => p.Name)
            .HasMaxLength(265);
    }
}