using Family.Api.Authorization;
using Family.Api.Data;
using Family.Api.Services;
using HotChocolate.Authorization;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Family.Api.GraphQL.Queries;

[ExtendObjectType<Query>]
public class FamilyRoleQueries
{
    [Authorize]
    public async Task<bool> IsFamilyAdmin(
        ClaimsPrincipal claimsPrincipal,
        [Service] IFamilyRoleAssignmentService roleAssignmentService,
        [Service] FamilyDbContext context,
        CancellationToken cancellationToken)
    {
        var subjectId = claimsPrincipal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(subjectId))
            return false;

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.KeycloakSubjectId == subjectId, cancellationToken);

        if (user == null)
            return false;

        return await roleAssignmentService.HasFamilyAdminRoleAsync(user.Id, cancellationToken);
    }

    [Authorize]
    public async Task<bool> IsFamilyUser(
        ClaimsPrincipal claimsPrincipal,
        [Service] IFamilyRoleAssignmentService roleAssignmentService,
        [Service] FamilyDbContext context,
        CancellationToken cancellationToken)
    {
        var subjectId = claimsPrincipal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(subjectId))
            return false;

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.KeycloakSubjectId == subjectId, cancellationToken);

        if (user == null)
            return false;

        return await roleAssignmentService.HasFamilyUserRoleAsync(user.Id, cancellationToken);
    }

    [Authorize]
    public async Task<List<string>> GetMyFamilyRoles(
        ClaimsPrincipal claimsPrincipal,
        [Service] IFamilyRoleAssignmentService roleAssignmentService,
        [Service] FamilyDbContext context,
        CancellationToken cancellationToken)
    {
        var subjectId = claimsPrincipal.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(subjectId))
            return new List<string>();

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.KeycloakSubjectId == subjectId, cancellationToken);

        if (user == null)
            return new List<string>();

        return await roleAssignmentService.GetUserFamilyRolesAsync(user.Id, cancellationToken);
    }

    [Authorize(Policy = Policies.FamilyAdmin)]
    public async Task<List<FamilyMemberRole>> GetFamilyMemberRoles(
        string familyId,
        [Service] FamilyDbContext context,
        CancellationToken cancellationToken)
    {
        var familyMembers = await context.FamilyMembers
            .Include(fm => fm.Family)
            .Where(fm => fm.FamilyId == familyId)
            .ToListAsync(cancellationToken);

        var result = new List<FamilyMemberRole>();

        foreach (var member in familyMembers)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.Id == member.UserId, cancellationToken);

            if (user != null)
            {
                result.Add(new FamilyMemberRole
                {
                    UserId = member.UserId,
                    UserEmail = user.Email,
                    UserName = $"{user.FirstName} {user.LastName}".Trim(),
                    Role = member.Role,
                    JoinedAt = member.JoinedAt,
                    IsOwner = member.Family?.OwnerId == member.UserId
                });
            }
        }

        return result;
    }
}

public class FamilyMemberRole
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public bool IsOwner { get; set; }
}