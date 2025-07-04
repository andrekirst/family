using Family.Api.Features.Families.DTOs;
using Family.Infrastructure.CQRS.Abstractions;
using FluentValidation;
using MediatR;

namespace Family.Api.Features.Families.Commands;

public record CreateFamilyCommand(string Name, Guid UserId, string CorrelationId) : ICommand<CreateFamilyResult>;

public class CreateFamilyResult : CommandResult
{
    public FamilyDto? Family { get; set; }
}

public class CreateFamilyCommandValidator : AbstractValidator<CreateFamilyCommand>
{
    public CreateFamilyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Family name is required")
            .MaximumLength(100)
            .WithMessage("Family name cannot exceed 100 characters");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.CorrelationId)
            .NotEmpty()
            .WithMessage("Correlation ID is required");
    }
}

public class CreateFamilyCommandHandler : IRequestHandler<CreateFamilyCommand, CreateFamilyResult>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IValidator<CreateFamilyCommand> _validator;

    public CreateFamilyCommandHandler(IFamilyRepository familyRepository, IValidator<CreateFamilyCommand> validator)
    {
        _familyRepository = familyRepository;
        _validator = validator;
    }

    public async Task<CreateFamilyResult> Handle(CreateFamilyCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var validationErrors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            
            return new CreateFamilyResult
            {
                IsSuccess = false,
                ValidationErrors = validationErrors
            };
        }

        // Check if user already has a family
        var existingFamily = await _familyRepository.GetByMemberIdAsync(request.UserId, cancellationToken);
        if (existingFamily != null)
        {
            return new CreateFamilyResult
            {
                IsSuccess = false,
                ErrorMessage = "User is already a member of a family"
            };
        }

        try
        {
            var family = Models.Family.Create(
                request.Name, 
                request.UserId, 
                request.UserId.ToString(), 
                request.CorrelationId);

            await _familyRepository.SaveAsync(family, cancellationToken);

            return new CreateFamilyResult
            {
                IsSuccess = true,
                Family = FamilyDto.FromDomain(family)
            };
        }
        catch (Exception ex)
        {
            return new CreateFamilyResult
            {
                IsSuccess = false,
                ErrorMessage = ex.Message
            };
        }
    }
}