using Api.Domain;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Database.Core;

public class NotificationMessageEntityConfigurationType : EntityTypeConfigurationBase<NotificationMessageEntity>
{
    public override void Configure(EntityTypeBuilder<NotificationMessageEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("NotificatioMessages", SchemaNames.Core);

        builder
            .Property(p => p.MarkedAsRead)
            .HasDefaultValue(false);

        builder
            .HasOne(p => p.FamilyMemberEntity)
            .WithMany(fm => fm.NotificationMessages)
            .HasForeignKey(p => p.FamilyMemberId)
            .IsRequired();

        builder
            .Property(p => p.MessageType)
            .HasMaxLength(DefaultLengths.Text)
            .IsRequired()
            .IsUnicode(false);
        
        builder
            .Property(p => p.Version)
            .HasMaxLength(DefaultLengths.Text)
            .IsRequired()
            .IsUnicode(false);
        
        builder
            .Property(p => p.Instance)
            .IsRequired();
    }
}