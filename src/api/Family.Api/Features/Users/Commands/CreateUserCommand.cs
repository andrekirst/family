using Family.Api.Data;
using Family.Api.Features.Users.DTOs;
using Family.Api.Models;
using Family.Infrastructure.CQRS.Abstractions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

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
    public CreateUserCommandValidator(IStringLocalizer<UserValidationMessages> localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(localizer["EmailRequired"])
            .EmailAddress().WithMessage(localizer["EmailInvalid"])
            .MaximumLength(255).WithMessage(x => localizer["EmailMaxLength", 255]);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(localizer["FirstNameRequired"])
            .MaximumLength(100).WithMessage(x => localizer["FirstNameMaxLength", 100]);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(localizer["LastNameRequired"])
            .MaximumLength(100).WithMessage(x => localizer["LastNameMaxLength", 100]);

        RuleFor(x => x.PreferredLanguage)
            .NotEmpty().WithMessage(localizer["PreferredLanguageRequired"])
            .Must(lang => lang == "de" || lang == "en")
            .WithMessage(localizer["PreferredLanguageInvalid"]);
    }
}

/// <summary>
/// Handler for CreateUserCommand
/// </summary>
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, CommandResult<UserDto>>
{
    private readonly FamilyDbContext _context;
    private readonly ILogger<CreateUserCommandHandler> _logger;
    private readonly IStringLocalizer<UserValidationMessages> _localizer;

    public CreateUserCommandHandler(
        FamilyDbContext context,
        ILogger<CreateUserCommandHandler> logger,
        IStringLocalizer<UserValidationMessages> localizer)
    {
        _context = context;
        _logger = logger;
        _localizer = localizer;
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
                return CommandResult<UserDto>.Failure(_localizer["EmailAlreadyExists"]);
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
            return CommandResult<UserDto>.Failure(_localizer["UserCreationFailed"]);
        }
    }
}