using Family.Api.Data;
using Family.Infrastructure.CQRS.Abstractions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Family.Api.Features.Users.Commands;

/// <summary>
/// Command to delete a user (soft delete by setting IsActive = false)
/// </summary>
public class DeleteUserCommand : ICommand<CommandResult>
{
    public Guid UserId { get; set; }
}

/// <summary>
/// Validator for DeleteUserCommand
/// </summary>
public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}

/// <summary>
/// Handler for DeleteUserCommand
/// </summary>
public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand, CommandResult>
{
    private readonly FamilyDbContext _context;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(
        FamilyDbContext context,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CommandResult> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user == null)
            {
                return CommandResult.Failure($"User with ID '{request.UserId}' not found");
            }

            if (!user.IsActive)
            {
                return CommandResult.Failure($"User with ID '{request.UserId}' is already deactivated");
            }

            // Soft delete: Set IsActive to false
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deactivated user {UserId}", user.Id);

            return CommandResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user {UserId}", request.UserId);
            return CommandResult.Failure("An error occurred while deactivating the user");
        }
    }
}