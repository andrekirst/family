using Api.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace Api.Extensions.QueryableExtensions;

public static class FamilyMemberQueryableExtensions
{
    public static Task<Guid> GetIdByAspNetUserId(this IQueryable<FamilyMember> query, string aspNetUserId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(aspNetUserId);
        
        return query
            .Where(f => f.AspNetUserId == aspNetUserId)
            .Select(f => f.Id)
            .SingleAsync(cancellationToken);
    }
}