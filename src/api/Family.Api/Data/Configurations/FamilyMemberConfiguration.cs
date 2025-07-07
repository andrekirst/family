using Family.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Family.Api.Data.Configurations;

public class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMemberEntity>
{
    public void Configure(EntityTypeBuilder<FamilyMemberEntity> builder)
    {
        builder.ToTable("family_members");
        
        // Composite primary key
        builder.HasKey(fm => new { fm.FamilyId, fm.UserId });
        
        builder.Property(fm => fm.FamilyId)
            .HasMaxLength(255)
            .IsRequired();
        
        builder.Property(fm => fm.UserId)
            .IsRequired();
        
        builder.Property(fm => fm.Role)
            .HasMaxLength(50)
            .IsRequired();
        
        builder.Property(fm => fm.JoinedAt)
            .IsRequired();
        
        // Add indexes
        builder.HasIndex(fm => fm.UserId)
            .HasDatabaseName("ix_family_members_user_id");
        
        builder.HasIndex(fm => fm.FamilyId)
            .HasDatabaseName("ix_family_members_family_id");
        
        builder.HasIndex(fm => fm.Role)
            .HasDatabaseName("ix_family_members_role");
        
        builder.HasIndex(fm => fm.JoinedAt)
            .HasDatabaseName("ix_family_members_joined_at");
        
        // Unique constraint: A user can only be in one family
        builder.HasIndex(fm => fm.UserId)
            .IsUnique()
            .HasDatabaseName("uq_family_members_user_id");
    }
}