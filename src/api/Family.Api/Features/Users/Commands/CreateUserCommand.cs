using Family.Api.Data;
using Family.Api.Features.Users.DTOs;
using Family.Api.Models;
using Family.Infrastructure.CQRS.Abstractions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Family.Api.Features.Users.Commands;

/// <summary>
/// Command to create a new user
/// </summary>
public class CreateUserCommand : ICommand<CommandResult<UserDto>>
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PreferredLanguage { get; set; } = "de";
    public string? KeycloakSubjectId { get; set; }
}

/// <summary>
/// Validator for CreateUserCommand
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be valid")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

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

        RuleFor(x => x.KeycloakSubjectId)
            .MaximumLength(255).WithMessage("Keycloak Subject ID must not exceed 255 characters");
    }
}

/// <summary>
/// Handler for CreateUserCommand
/// </summary>
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, CommandResult<UserDto>>
{
    private readonly FamilyDbContext _context;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        FamilyDbContext context,
        ILogger<CreateUserCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CommandResult<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user with email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (existingUser != null)
            {
                return CommandResult<UserDto>.Failure($"User with email '{request.Email}' already exists");
            }

            // Check if user with Keycloak Subject ID already exists
            if (!string.IsNullOrEmpty(request.KeycloakSubjectId))
            {
                var existingKeycloakUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.KeycloakSubjectId == request.KeycloakSubjectId, cancellationToken);

                if (existingKeycloakUser != null)
                {
                    return CommandResult<UserDto>.Failure($"User with Keycloak Subject ID '{request.KeycloakSubjectId}' already exists");
                }
            }

            // Create new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PreferredLanguage = request.PreferredLanguage,
                KeycloakSubjectId = request.KeycloakSubjectId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created user {UserId} with email {Email}", user.Id, user.Email);

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
                Roles = []
            };

            return CommandResult<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with email {Email}", request.Email);
            return CommandResult<UserDto>.Failure("An error occurred while creating the user");
        }
    }
}