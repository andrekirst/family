using Family.Api.Data;
using Family.Api.Features.Users.DTOs;
using Family.Infrastructure.CQRS.Abstractions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Family.Api.Features.Users.Commands;

/// <summary>
/// Command to update an existing user
/// </summary>
public class UpdateUserCommand : ICommand<CommandResult<UserDto>>
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = "de";
    public bool IsActive { get; set; }
}

/// <summary>
/// Validator for UpdateUserCommand
/// </summary>
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.PreferredLanguage)
            .NotEmpty().WithMessage("Preferred language is required")
            .Must(lang => lang == "de" || lang == "en")
            .WithMessage("Preferred language must be 'de' or 'en'");
    }
}

/// <summary>
/// Handler for UpdateUserCommand
/// </summary>
public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, CommandResult<UserDto>>
{
    private readonly FamilyDbContext _context;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(
        FamilyDbContext context,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CommandResult<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return CommandResult<UserDto>.Failure($"User with ID '{request.UserId}' not found");
            }

            // Update user properties
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PreferredLanguage = request.PreferredLanguage;
            user.IsActive = request.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated user {UserId}", user.Id);

            // Map to DTO
            var userDto = new UserDto
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

            return CommandResult<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", request.UserId);
            return CommandResult<UserDto>.Failure("An error occurred while updating the user");
        }
    }
}