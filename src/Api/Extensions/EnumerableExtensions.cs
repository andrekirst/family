using System.Diagnostics.CodeAnalysis;

namespace Api.Extensions;

public static class EnumerableExtensions
{
    public static void ThrowIfNullOrEmpty<T>([NotNull] this IEnumerable<T>? enumerable)
    {
        ArgumentNullException.ThrowIfNull(enumerable);
        _ = enumerable.TryGetNonEnumeratedCount(out var count);
        
        if (count == 0)
        {
            throw new ArgumentException("Can not be empty", nameof(enumerable));
        }
    }
}