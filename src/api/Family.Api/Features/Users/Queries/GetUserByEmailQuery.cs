using Family.Api.Data;
using Family.Api.Features.Users.DTOs;
using Family.Infrastructure.CQRS.Abstractions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Family.Api.Features.Users.Queries;

/// <summary>
/// Query to get a user by email address
/// </summary>
public class GetUserByEmailQuery : IQuery<UserDto?>
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Validator for GetUserByEmailQuery
/// </summary>
public class GetUserByEmailQueryValidator : AbstractValidator<GetUserByEmailQuery>
{
    public GetUserByEmailQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be valid");
    }
}

/// <summary>
/// Handler for GetUserByEmailQuery
/// </summary>
public class GetUserByEmailQueryHandler : IQueryHandler<GetUserByEmailQuery, UserDto?>
{
    private readonly FamilyDbContext _context;
    private readonly ILogger<GetUserByEmailQueryHandler> _logger;

    public GetUserByEmailQueryHandler(
        FamilyDbContext context,
        ILogger<GetUserByEmailQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserDto?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive, cancellationToken);

            if (user == null)
            {
                _logger.LogDebug("User with email {Email} not found or inactive", request.Email);
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
            _logger.LogError(ex, "Error retrieving user by email {Email}", request.Email);
            throw;
        }
    }
}