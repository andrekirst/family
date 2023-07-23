using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Domain.Core;

public static class ColorEntityTypeExtensions
{
    public static void ConfigureColor<TType>(
        this EntityTypeBuilder<TType> builder,
        Expression<Func<TType, Color>> colorProperty,
        string? targetColumnName = "Color")
        where TType : class
    {
        builder.OwnsOne(colorProperty!)
            .Property(_ => _.Value)
            .HasColumnName(targetColumnName ?? "Color")
            .IsUnicode(false)
            .HasMaxLength(256);
    }
}