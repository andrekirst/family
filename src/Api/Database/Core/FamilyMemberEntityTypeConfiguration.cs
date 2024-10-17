using Api.Domain;
using Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Core;

public class FamilyMemberEntityTypeConfiguration : EntityTypeConfigurationBase<FamilyMemberEntity>
{
    public const string EntityTableName = "FamilyMembers";

    public override void Configure(EntityTypeBuilder<FamilyMemberEntity> builder)
    {
        base.Configure(builder);
        
        builder
            .ToTable(EntityTableName, SchemaNames.Core);

        builder
            .Property(p => p.FirstName)
            .HasMaxLength(DefaultLengths.Text);

        builder
            .Property(p => p.LastName)
            .HasMaxLength(DefaultLengths.Text);

        builder
            .Property(p => p.AspNetUserId)
            .HasMaxLength(450);

        builder
            .HasMany(fm => fm.CreatedByDomainEventEntries)
            .WithOne(de => de.CreatedByFamilyMember)
            .HasForeignKey(de => de.CreatedByFamilyMemberId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasMany(fm => fm.CreatedForDomainEventEntries)
            .WithOne(de => de.CreatedForFamilyMember)
            .HasForeignKey(de => de.CreatedForFamilyMemberId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}