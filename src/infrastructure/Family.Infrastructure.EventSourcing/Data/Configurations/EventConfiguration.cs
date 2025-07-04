using Family.Infrastructure.EventSourcing.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Family.Infrastructure.EventSourcing.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.AggregateId)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.AggregateType)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.EventData)
            .IsRequired()
            .HasColumnType("jsonb");
        
        builder.Property(e => e.Metadata)
            .IsRequired()
            .HasDefaultValue("{}")
            .HasColumnType("jsonb");
        
        builder.Property(e => e.Version)
            .IsRequired();
        
        builder.Property(e => e.Timestamp)
            .IsRequired();
        
        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.CorrelationId)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.CausationId)
            .IsRequired()
            .HasMaxLength(100);
        
        // Indexes for performance optimization
        builder.HasIndex(e => e.AggregateId)
            .HasDatabaseName("ix_events_aggregate_id");
        
        builder.HasIndex(e => new { e.AggregateId, e.Version })
            .IsUnique()
            .HasDatabaseName("ix_events_aggregate_id_version");
        
        builder.HasIndex(e => e.AggregateType)
            .HasDatabaseName("ix_events_aggregate_type");
        
        builder.HasIndex(e => e.EventType)
            .HasDatabaseName("ix_events_event_type");
        
        builder.HasIndex(e => e.Timestamp)
            .HasDatabaseName("ix_events_timestamp");
        
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_events_user_id");
        
        builder.HasIndex(e => e.CorrelationId)
            .HasDatabaseName("ix_events_correlation_id");
    }
}