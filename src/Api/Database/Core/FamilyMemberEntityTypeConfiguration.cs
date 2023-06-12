using Api.Domain;
using Api.Domain.Core;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Core;

public class FamilyMemberEntityTypeConfiguration : EntityTypeConfigurationBase<FamilyMember>
{
    public override void Configure(EntityTypeBuilder<FamilyMember> builder)
    {
        base.Configure(builder);

        builder
            .ToTable("FamilyMembers", SchemaNames.Core);

        builder
            .Property(p => p.FirstName)
            .HasMaxLength(DefaultLengths.Text);

        builder
            .Property(p => p.LastName)
            .HasMaxLength(DefaultLengths.Text);
    }
}