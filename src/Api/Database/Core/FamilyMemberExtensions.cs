using Api.Domain.Core;
using Microsoft.EntityFrameworkCore;

namespace Api.Database.Core;

public static class FamilyMemberExtensions
{
    public static Task<bool> Exists(this IQueryable<FamilyMember> query, int id, CancellationToken cancellationToken = default)
        => query.AnyAsync(f => f.Id == id, cancellationToken);
}