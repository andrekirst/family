using Family.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Family.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => e.KeycloakSubjectId)
            .IsUnique()
            .HasDatabaseName("ix_users_keycloak_subject_id");
        
        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        builder.Property(e => e.Id)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(e => e.KeycloakSubjectId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.FirstName)
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .HasMaxLength(100);

        builder.Property(e => e.PreferredLanguage)
            .HasMaxLength(10)
            .HasDefaultValue("de");

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);
    }
}