using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Database;

public static class ApplicationDbContextQueryExtensions
{
    public static Task<bool> Exists<TEntity>(this IQueryable<TEntity> query, Guid id, CancellationToken cancellationToken = default)
        where TEntity : IEntityId
        => query.AnyAsync(f => f.Id == id, cancellationToken);
}