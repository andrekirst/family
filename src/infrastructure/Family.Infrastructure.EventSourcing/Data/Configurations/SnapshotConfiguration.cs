using Family.Infrastructure.EventSourcing.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Family.Infrastructure.EventSourcing.Data.Configurations;

public class SnapshotConfiguration : IEntityTypeConfiguration<Snapshot>
{
    public void Configure(EntityTypeBuilder<Snapshot> builder)
    {
        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.AggregateId)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(s => s.AggregateType)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(s => s.Data)
            .IsRequired()
            .HasColumnType("jsonb");
        
        builder.Property(s => s.Version)
            .IsRequired();
        
        builder.Property(s => s.Timestamp)
            .IsRequired();
        
        // Indexes for performance optimization
        builder.HasIndex(s => s.AggregateId)
            .HasDatabaseName("ix_snapshots_aggregate_id");
        
        builder.HasIndex(s => new { s.AggregateId, s.Version })
            .IsUnique()
            .HasDatabaseName("ix_snapshots_aggregate_id_version");
        
        builder.HasIndex(s => s.AggregateType)
            .HasDatabaseName("ix_snapshots_aggregate_type");
        
        builder.HasIndex(s => s.Timestamp)
            .HasDatabaseName("ix_snapshots_timestamp");
    }
}