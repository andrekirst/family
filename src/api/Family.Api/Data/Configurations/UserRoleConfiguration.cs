using Family.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Family.Api.Data.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.UserId, e.RoleName })
            .IsUnique()
            .HasDatabaseName("ix_user_roles_user_id_role_name");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.RoleName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Source)
            .HasMaxLength(100)
            .HasDefaultValue("keycloak");

        // Foreign key relationship
        builder.HasOne(e => e.User)
            .WithMany(e => e.UserRoles)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}