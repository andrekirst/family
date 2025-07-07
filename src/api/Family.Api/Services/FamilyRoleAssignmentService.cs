using Family.Api.Authorization;
using Family.Api.Data;
using Family.Api.Features.Families.DomainEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Family.Api.Services;

public interface IFamilyRoleAssignmentService
{
    Task AssignFamilyAdminRoleAsync(Guid userId, string familyId, CancellationToken cancellationToken = default);
    Task AssignFamilyUserRoleAsync(Guid userId, string familyId, CancellationToken cancellationToken = default);
    Task<bool> HasFamilyAdminRoleAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasFamilyUserRoleAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserFamilyRolesAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class FamilyRoleAssignmentService : IFamilyRoleAssignmentService
{
    private readonly FamilyDbContext _dbContext;
    private readonly IKeycloakService _keycloakService;
    private readonly ILogger<FamilyRoleAssignmentService> _logger;

    public FamilyRoleAssignmentService(
        FamilyDbContext dbContext,
        IKeycloakService keycloakService,
        ILogger<FamilyRoleAssignmentService> logger)
    {
        _dbContext = dbContext;
        _keycloakService = keycloakService;
        _logger = logger;
    }

    public async Task AssignFamilyAdminRoleAsync(Guid userId, string familyId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Assigning FamilyAdmin role to user {UserId} for family {FamilyId}", userId, familyId);

            // Get user from database
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for role assignment", userId);
                return;
            }

            // Check if user is member of the family
            var familyMember = await _dbContext.FamilyMembers
                .FirstOrDefaultAsync(fm => fm.UserId == userId && fm.FamilyId == familyId, cancellationToken);

            if (familyMember == null)
            {
                _logger.LogWarning("User {UserId} is not a member of family {FamilyId}", userId, familyId);
                return;
            }

            // Update role in database
            familyMember.Role = Features.Families.Models.FamilyRole.FamilyAdmin.ToString();
            await _dbContext.SaveChangesAsync(cancellationToken);

            // TODO: Assign role in Keycloak
            // await _keycloakService.AssignRoleToUserAsync(user.KeycloakSubjectId, Roles.FamilyAdmin);

            _logger.LogInformation("Successfully assigned FamilyAdmin role to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning FamilyAdmin role to user {UserId} for family {FamilyId}", userId, familyId);
            throw;
        }
    }

    public async Task AssignFamilyUserRoleAsync(Guid userId, string familyId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Assigning FamilyUser role to user {UserId} for family {FamilyId}", userId, familyId);

            // Get user from database
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found for role assignment", userId);
                return;
            }

            // Check if user is member of the family
            var familyMember = await _dbContext.FamilyMembers
                .FirstOrDefaultAsync(fm => fm.UserId == userId && fm.FamilyId == familyId, cancellationToken);

            if (familyMember == null)
            {
                _logger.LogWarning("User {UserId} is not a member of family {FamilyId}", userId, familyId);
                return;
            }

            // Update role in database only if not already admin
            if (familyMember.Role != Features.Families.Models.FamilyRole.FamilyAdmin.ToString())
            {
                familyMember.Role = Features.Families.Models.FamilyRole.FamilyUser.ToString();
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // TODO: Assign role in Keycloak
            // await _keycloakService.AssignRoleToUserAsync(user.KeycloakSubjectId, Roles.FamilyUser);

            _logger.LogInformation("Successfully assigned FamilyUser role to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning FamilyUser role to user {UserId} for family {FamilyId}", userId, familyId);
            throw;
        }
    }

    public async Task<bool> HasFamilyAdminRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var hasAdminRole = await _dbContext.FamilyMembers
                .AnyAsync(fm => fm.UserId == userId && 
                               fm.Role == Features.Families.Models.FamilyRole.FamilyAdmin.ToString(), 
                         cancellationToken);

            return hasAdminRole;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking FamilyAdmin role for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> HasFamilyUserRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var hasFamilyRole = await _dbContext.FamilyMembers
                .AnyAsync(fm => fm.UserId == userId, cancellationToken);

            return hasFamilyRole;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking family role for user {UserId}", userId);
            return false;
        }
    }

    public async Task<List<string>> GetUserFamilyRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = new List<string>();

            var familyMember = await _dbContext.FamilyMembers
                .FirstOrDefaultAsync(fm => fm.UserId == userId, cancellationToken);

            if (familyMember != null)
            {
                // Always add FamilyUser role if user is member of any family
                roles.Add(Roles.FamilyUser);

                // Add FamilyAdmin role if user is admin
                if (familyMember.Role == Features.Families.Models.FamilyRole.FamilyAdmin.ToString())
                {
                    roles.Add(Roles.FamilyAdmin);
                }
            }

            return roles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting family roles for user {UserId}", userId);
            return new List<string>();
        }
    }
}

/// <summary>
/// Event handler that automatically assigns roles when family events occur
/// </summary>
public class FamilyRoleAssignmentEventHandler : 
    INotificationHandler<FamilyCreated>,
    INotificationHandler<FamilyMemberAdded>,
    INotificationHandler<FamilyAdminAssigned>
{
    private readonly IFamilyRoleAssignmentService _roleAssignmentService;
    private readonly ILogger<FamilyRoleAssignmentEventHandler> _logger;

    public FamilyRoleAssignmentEventHandler(
        IFamilyRoleAssignmentService roleAssignmentService,
        ILogger<FamilyRoleAssignmentEventHandler> logger)
    {
        _roleAssignmentService = roleAssignmentService;
        _logger = logger;
    }

    public async Task Handle(FamilyCreated notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing FamilyCreated event for automatic role assignment");
        
        // Family owner automatically becomes admin
        await _roleAssignmentService.AssignFamilyAdminRoleAsync(
            notification.OwnerId, 
            notification.AggregateId, 
            cancellationToken);
    }

    public async Task Handle(FamilyMemberAdded notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing FamilyMemberAdded event for automatic role assignment");
        
        // New family members get FamilyUser role by default
        if (notification.Role == Features.Families.Models.FamilyRole.FamilyUser.ToString())
        {
            await _roleAssignmentService.AssignFamilyUserRoleAsync(
                notification.MemberUserId, 
                notification.AggregateId, 
                cancellationToken);
        }
        else if (notification.Role == Features.Families.Models.FamilyRole.FamilyAdmin.ToString())
        {
            await _roleAssignmentService.AssignFamilyAdminRoleAsync(
                notification.MemberUserId, 
                notification.AggregateId, 
                cancellationToken);
        }
    }

    public async Task Handle(FamilyAdminAssigned notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing FamilyAdminAssigned event for role assignment");
        
        // Parse UserId from string (comes from domain event)
        if (Guid.TryParse(notification.UserId, out var userId))
        {
            await _roleAssignmentService.AssignFamilyAdminRoleAsync(
                userId, 
                notification.AggregateId, 
                cancellationToken);
        }
        else
        {
            _logger.LogWarning("Invalid UserId format in FamilyAdminAssigned event: {UserId}", notification.UserId);
        }
    }
}