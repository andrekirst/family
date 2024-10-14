using Api.Database;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Core.Authentication.Google;

public class GoogleAccountEntityTypeConfiguration : EntityTypeConfigurationBase<GoogleAccount>
{
    public const string EntityTableName = "GoogleAccounts";
    
    public override void Configure(EntityTypeBuilder<GoogleAccount> builder)
    {
        base.Configure(builder);

        builder
            .ToTable(EntityTableName, SchemaNames.Core);

        builder
            .Property(p => p.AccessToken)
            .HasMaxLength(512);

        builder
            .Property(p => p.GoogleId)
            .HasMaxLength(512);

        builder
            .Property(p => p.UserId)
            .HasMaxLength(450);
    }
}