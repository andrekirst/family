using Family.Api.Data;
using Family.Api.Features.Users.DTOs;
using Family.Infrastructure.CQRS.Abstractions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Family.Api.Features.Users.Queries;

/// <summary>
/// Query to get a user by ID
/// </summary>
public class GetUserByIdQuery : IQuery<UserDto?>
{
    public Guid UserId { get; set; }
}

/// <summary>
/// Validator for GetUserByIdQuery
/// </summary>
public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}

/// <summary>
/// Handler for GetUserByIdQuery
/// </summary>
public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto?>
{
    private readonly FamilyDbContext _context;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(
        FamilyDbContext context,
        ILogger<GetUserByIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId && u.IsActive, cancellationToken);

            if (user == null)
            {
                _logger.LogDebug("User {UserId} not found or inactive", request.UserId);
                return null;
            }

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PreferredLanguage = user.PreferredLanguage,
                KeycloakSubjectId = user.KeycloakSubjectId,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = user.UserRoles.Select(r => new UserRoleDto
                {
                    Id = r.Id,
                    RoleName = r.RoleName,
                    Source = r.Source,
                    CreatedAt = r.CreatedAt
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user {UserId}", request.UserId);
            throw;
        }
    }
}