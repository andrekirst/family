using Api.Database.Core;
using Microsoft.EntityFrameworkCore;

namespace Api.Extensions.QueryableExtensions;

public static class FamilyMemberQueryableExtensions
{
    public static Task<Guid> GetIdByAspNetUserId(this IQueryable<FamilyMemberEntity> query, string aspNetUserId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(aspNetUserId);
        
        return query
            .Where(f => f.AspNetUserId == aspNetUserId)
            .Select(f => f.Id)
            .SingleAsync(cancellationToken);
    }
}