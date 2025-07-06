using Family.Api.Features.Families.DTOs;
using Family.Infrastructure.CQRS.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Family.Api.Features.Families.Commands;

public record CreateFamilyCommand(string Name, Guid UserId, string CorrelationId) : ICommand<CreateFamilyResult>;

public class CreateFamilyResult : CommandResult
{
    public FamilyDto? Family { get; set; }
}

public class CreateFamilyCommandValidator : AbstractValidator<CreateFamilyCommand>
{
    public CreateFamilyCommandValidator(IStringLocalizer<CreateFamilyCommandValidator> localizer)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(localizer["FamilyNameRequired"])
            .MaximumLength(100)
            .WithMessage(localizer["FamilyNameMaxLength", 100]);

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(localizer["CreatedByRequired"]);

        RuleFor(x => x.CorrelationId)
            .NotEmpty()
            .WithMessage("Correlation ID is required"); // Keep internal, not user-facing
    }
}

public class CreateFamilyCommandHandler : IRequestHandler<CreateFamilyCommand, CreateFamilyResult>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IValidator<CreateFamilyCommand> _validator;
    private readonly IStringLocalizer<CreateFamilyCommandHandler> _localizer;

    public CreateFamilyCommandHandler(
        IFamilyRepository familyRepository, 
        IValidator<CreateFamilyCommand> validator,
        IStringLocalizer<CreateFamilyCommandHandler> localizer)
    {
        _familyRepository = familyRepository;
        _validator = validator;
        _localizer = localizer;
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
                ErrorMessage = _localizer["UserAlreadyHasFamily"]
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
                ErrorMessage = _localizer["FamilyCreationFailed"]
            };
        }
    }
}