using Family.Api.Data;
using Family.Api.Models;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Family.Api.GraphQL.Queries;

[ExtendObjectType<Query>]
public class UserQueries
{
    [Authorize]
    public async Task<User?> GetCurrentUserAsync(
        ClaimsPrincipal claimsPrincipal,
        FamilyDbContext context,
        CancellationToken cancellationToken)
    {
        var subjectId = claimsPrincipal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(subjectId))
            return null;

        return await context.Users
            .FirstOrDefaultAsync(u => u.KeycloakSubjectId == subjectId, cancellationToken);
    }

    [Authorize(Policy = "FamilyAdmin")]
    public async Task<IQueryable<User>> GetUsersAsync(FamilyDbContext context)
    {
        return context.Users.Where(u => u.IsActive);
    }

    [Authorize]
    public async Task<User?> GetUserByIdAsync(
        Guid id,
        ClaimsPrincipal claimsPrincipal,
        FamilyDbContext context,
        CancellationToken cancellationToken)
    {
        var currentUserSubId = claimsPrincipal.FindFirst("sub")?.Value;
        var isAdmin = claimsPrincipal.HasClaim("family_roles", "family-admin");

        // Users can only see their own profile unless they are admin
        if (!isAdmin)
        {
            var currentUser = await context.Users
                .FirstOrDefaultAsync(u => u.KeycloakSubjectId == currentUserSubId, cancellationToken);
            
            if (currentUser?.Id != id)
                return null;
        }

        return await context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive, cancellationToken);
    }
}

public class Query
{
    public string Hello() => "Hello from Family API!";
}