using Api.Domain;
using Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Calendar;

public class CalendarEntityTypeConfiguration : EntityTypeConfigurationBase<CalendarEntity>
{
    public override void Configure(EntityTypeBuilder<CalendarEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("Calendars", SchemaNames.Calendar);

        builder
            .Property(c => c.Name)
            .HasMaxLength(DefaultLengths.Text);

        builder
            .HasOne(c => c.FamilyMemberEntity)
            .WithMany(fm => fm.Calendars)
            .HasForeignKey(c => c.FamilyMemberId)
            .IsRequired();
    }
}