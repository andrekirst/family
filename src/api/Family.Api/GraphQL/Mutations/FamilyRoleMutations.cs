using Family.Api.Authorization;
using Family.Api.Data;
using Family.Api.Services;
using HotChocolate.Authorization;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace Family.Api.GraphQL.Mutations;

[ExtendObjectType<Mutation>]
public class FamilyRoleMutations
{
    [Authorize(Policy = Policies.FamilyAdmin)]
    public async Task<AssignRoleResult> AssignFamilyAdminRole(
        AssignRoleInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IFamilyRoleAssignmentService roleAssignmentService,
        [Service] FamilyDbContext context,
        [Service] IStringLocalizer<FamilyRoleMutations> localizer,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify the current user is admin of the family
            var currentUserSubId = claimsPrincipal.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(currentUserSubId))
            {
                return new AssignRoleResult
                {
                    Success = false,
                    ErrorMessage = "Benutzer nicht authentifiziert"
                };
            }

            var currentUser = await context.Users
                .FirstOrDefaultAsync(u => u.KeycloakSubjectId == currentUserSubId, cancellationToken);

            if (currentUser == null)
            {
                return new AssignRoleResult
                {
                    Success = false,
                    ErrorMessage = "Aktueller Benutzer nicht gefunden"
                };
            }

            // Check if current user is admin of the target family
            var currentUserIsFamilyAdmin = await context.FamilyMembers
                .AnyAsync(fm => fm.UserId == currentUser.Id && 
                               fm.FamilyId == input.FamilyId &&
                               fm.Role == Features.Families.Models.FamilyRole.FamilyAdmin.ToString(), 
                         cancellationToken);

            if (!currentUserIsFamilyAdmin)
            {
                return new AssignRoleResult
                {
                    Success = false,
                    ErrorMessage = localizer["NotAuthorizedToAssignRoles"]
                };
            }

            // Verify target user exists and is member of the family
            var targetUser = await context.Users
                .FirstOrDefaultAsync(u => u.Id == input.UserId, cancellationToken);

            if (targetUser == null)
            {
                return new AssignRoleResult
                {
                    Success = false,
                    ErrorMessage = localizer["TargetUserNotFound"]
                };
            }

            var targetUserIsFamilyMember = await context.FamilyMembers
                .AnyAsync(fm => fm.UserId == input.UserId && fm.FamilyId == input.FamilyId, cancellationToken);

            if (!targetUserIsFamilyMember)
            {
                return new AssignRoleResult
                {
                    Success = false,
                    ErrorMessage = localizer["UserNotMemberOfFamily"]
                };
            }

            // Assign admin role
            await roleAssignmentService.AssignFamilyAdminRoleAsync(input.UserId, input.FamilyId, cancellationToken);

            return new AssignRoleResult
            {
                Success = true,
                Message = localizer["AdminRoleAssignedSuccessfully", targetUser.FirstName, targetUser.LastName]
            };
        }
        catch (Exception ex)
        {
            return new AssignRoleResult
            {
                Success = false,
                ErrorMessage = localizer["ErrorAssigningAdminRole"]
            };
        }
    }

    [Authorize(Policy = Policies.FamilyAdmin)]
    public async Task<AssignRoleResult> RevokeFamilyAdminRole(
        AssignRoleInput input,
        ClaimsPrincipal claimsPrincipal,
        [Service] IFamilyRoleAssignmentService roleAssignmentService,
        [Service] FamilyDbContext context,
        [Service] IStringLocalizer<FamilyRoleMutations> localizer,
        CancellationToken cancellationToken)
    {
        try
        {
            // Verify the current user is admin of the family
            var currentUserSubId = claimsPrincipal.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(currentUserSubId))
            {
                return new AssignRoleResult
                {
                    Success = false,
                    ErrorMessage = "Benutzer nicht authentifiziert"
                };
            }

            var currentUser = await context.Users
                .FirstOrDefaultAsync(u => u.KeycloakSubjectId == currentUserSubId, cancellationToken);

            if (currentUser == null)
            {
                return new AssignRoleResult
                {
                    Success = false,
                    ErrorMessage = "Aktueller Benutzer nicht gefunden"
                };
            }

            // Check if current user is admin of the target family
            var currentUserIsFamilyAdmin = await context.FamilyMembers
                .AnyAsync(fm => fm.UserId == currentUser.Id && 
                               fm.FamilyId == input.FamilyId &&
                               fm.Role == Features.Families.Models.FamilyRole.FamilyAdmin.ToString(), 
                         cancellationToken);

            if (!currentUserIsFamilyAdmin)
            {
                return new AssignRoleResult
                {
                    Success = false,
                    ErrorMessage = "Nicht berechtigt, Rollen in dieser Familie zu widerrufen"
                };
            }

            // Check if target user is the family owner (can't revoke owner's admin role)
            var family = await context.Families
                .FirstOrDefaultAsync(f => f.Id == input.FamilyId, cancellationToken);

            if (family?.OwnerId == input.UserId)
            {
                return new AssignRoleResult
                {
                    Success = false,
                    ErrorMessage = "Die Administratorrolle des Familienbesitzers kann nicht widerrufen werden"
                };
            }

            // Don't allow users to revoke their own admin role if they are the only admin
            var adminCount = await context.FamilyMembers
                .CountAsync(fm => fm.FamilyId == input.FamilyId && 
                                 fm.Role == Features.Families.Models.FamilyRole.FamilyAdmin.ToString(), 
                           cancellationToken);

            if (currentUser.Id == input.UserId && adminCount <= 1)
            {
                return new AssignRoleResult
                {
                    Success = false,
                    ErrorMessage = "Sie können Ihre Administratorrolle nicht widerrufen, da Sie der einzige Administrator sind"
                };
            }

            // Revoke admin role (downgrade to family user)
            await roleAssignmentService.AssignFamilyUserRoleAsync(input.UserId, input.FamilyId, cancellationToken);

            var targetUser = await context.Users
                .FirstOrDefaultAsync(u => u.Id == input.UserId, cancellationToken);

            return new AssignRoleResult
            {
                Success = true,
                Message = $"Administratorrolle von {targetUser?.FirstName} {targetUser?.LastName} erfolgreich widerrufen"
            };
        }
        catch (Exception ex)
        {
            return new AssignRoleResult
            {
                Success = false,
                ErrorMessage = "Fehler beim Widerrufen der Administratorrolle"
            };
        }
    }
}

public class AssignRoleInput
{
    public string FamilyId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class AssignRoleResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
}