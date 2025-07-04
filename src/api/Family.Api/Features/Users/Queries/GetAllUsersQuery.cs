using Family.Api.Data;
using Family.Api.Features.Users.DTOs;
using Family.Infrastructure.CQRS.Abstractions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Family.Api.Features.Users.Queries;

/// <summary>
/// Query to get all active users with optional pagination
/// </summary>
public class GetAllUsersQuery : IQuery<List<UserDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public bool IncludeInactive { get; set; } = false;
}

/// <summary>
/// Validator for GetAllUsersQuery
/// </summary>
public class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
{
    public GetAllUsersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");
    }
}

/// <summary>
/// Handler for GetAllUsersQuery
/// </summary>
public class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly FamilyDbContext _context;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(
        FamilyDbContext context,
        ILogger<GetAllUsersQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.UserRoles)
                .AsNoTracking();

            if (!request.IncludeInactive)
            {
                query = query.Where(u => u.IsActive);
            }

            var users = await query
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var userDtos = users.Select(user => new UserDto
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
            }).ToList();

            _logger.LogDebug("Retrieved {Count} users (page {PageNumber}, size {PageSize})", 
                userDtos.Count, request.PageNumber, request.PageSize);

            return userDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users (page {PageNumber}, size {PageSize})", 
                request.PageNumber, request.PageSize);
            throw;
        }
    }
}