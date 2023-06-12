using Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure;

public class EntityTypeConfigurationBase<T> : IEntityTypeConfiguration<T>
    where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(table => table.Id);

        builder
            .Property(p => p.CreatedBy)
            .HasMaxLength(DefaultLengths.CreatedBy);

        builder
            .Property(p => p.ChangedBy)
            .HasMaxLength(DefaultLengths.ChangedBy);

        builder
            .Property(p => p.ConcurrencyToken)
            .IsUnicode(false)
            .HasMaxLength(DefaultLengths.ConcurrencyToken)
            .IsConcurrencyToken();

        builder
            .Property(p => p.RowVersion)
            .IsUnicode(false)
            .HasMaxLength(DefaultLengths.RowVersion)
            .IsRowVersion();
    }
}