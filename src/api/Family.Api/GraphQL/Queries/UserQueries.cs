using Family.Api.Authorization;
using Family.Api.Data;
using Family.Api.Features.Users.Services;
using Family.Api.Models;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Family.Api.GraphQL.Queries;

[ExtendObjectType<Query>]
public class UserQueries
{
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

    public async Task<IQueryable<User>> GetUsersAsync(FamilyDbContext context)
    {
        return context.Users.Where(u => u.IsActive);
    }

    public async Task<User?> GetUserByIdAsync(
        Guid id,
        ClaimsPrincipal claimsPrincipal,
        FamilyDbContext context,
        CancellationToken cancellationToken)
    {
        var currentUserSubId = claimsPrincipal.FindFirst("sub")?.Value;
        var isAdmin = claimsPrincipal.HasClaim(Claims.FamilyRoles, Roles.FamilyAdmin);

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

    [Authorize]
    public async Task<bool> IsFirstTimeUser(
        ClaimsPrincipal claimsPrincipal,
        [Service] IFirstTimeUserService firstTimeUserService,
        [Service] FamilyDbContext context,
        CancellationToken cancellationToken)
    {
        var subjectId = claimsPrincipal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(subjectId))
            return true; // If no valid token, treat as first time

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.KeycloakSubjectId == subjectId, cancellationToken);

        if (user == null)
            return true; // User not found in database, definitely first time

        return await firstTimeUserService.IsFirstTimeUserAsync(user.Id, cancellationToken);
    }

    [Authorize]
    public async Task<FirstTimeUserInfo> GetFirstTimeUserInfo(
        ClaimsPrincipal claimsPrincipal,
        [Service] IFirstTimeUserService firstTimeUserService,
        [Service] FamilyDbContext context,
        CancellationToken cancellationToken)
    {
        var subjectId = claimsPrincipal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(subjectId))
        {
            return new FirstTimeUserInfo(
                IsFirstTime: true,
                HasFamily: false
            );
        }

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.KeycloakSubjectId == subjectId, cancellationToken);

        if (user == null)
        {
            return new FirstTimeUserInfo(
                IsFirstTime: true,
                HasFamily: false
            );
        }

        return await firstTimeUserService.GetFirstTimeUserInfoAsync(user.Id, cancellationToken);
    }
}

public class Query
{
    public string Hello() => "Hello from Family API!";
}