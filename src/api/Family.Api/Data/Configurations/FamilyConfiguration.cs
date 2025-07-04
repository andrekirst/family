using Family.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Family.Api.Data.Configurations;

public class FamilyConfiguration : IEntityTypeConfiguration<FamilyEntity>
{
    public void Configure(EntityTypeBuilder<FamilyEntity> builder)
    {
        builder.ToTable("families");
        
        builder.HasKey(f => f.Id);
        
        builder.Property(f => f.Id)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(f => f.Name)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(f => f.OwnerId)
            .IsRequired();
        
        builder.Property(f => f.CreatedAt)
            .IsRequired();
        
        builder.Property(f => f.UpdatedAt)
            .IsRequired();
        
        // Configure the relationship with FamilyMembers
        builder.HasMany(f => f.Members)
            .WithOne(fm => fm.Family)
            .HasForeignKey(fm => fm.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Add indexes
        builder.HasIndex(f => f.OwnerId)
            .HasDatabaseName("ix_families_owner_id");
        
        builder.HasIndex(f => f.Name)
            .HasDatabaseName("ix_families_name");
        
        builder.HasIndex(f => f.CreatedAt)
            .HasDatabaseName("ix_families_created_at");
    }
}