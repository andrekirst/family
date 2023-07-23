using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure;

public static class EntityTypeConfigurationExtensions
{
    public static void ConfigureEnumAsString<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, Enum>> @enum) where TEntity : class
    {
        builder
            .Property(@enum)
            .HasConversion<string>()
            .IsUnicode(false)
            .HasMaxLength(256);
    }
}