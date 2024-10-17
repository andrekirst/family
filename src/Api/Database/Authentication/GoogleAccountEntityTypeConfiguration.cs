using Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Authentication;

public class GoogleAccountEntityTypeConfiguration : EntityTypeConfigurationBase<GoogleAccountEntity>
{
    public override void Configure(EntityTypeBuilder<GoogleAccountEntity> builder)
    {
        base.Configure(builder);

        builder
            .ToTable("GoogleAccounts", SchemaNames.Authentication);

        builder
            .Property(p => p.GoogleId)
            .HasMaxLength(512);

        builder
            .Property(p => p.UserId)
            .HasMaxLength(450);
    }
}