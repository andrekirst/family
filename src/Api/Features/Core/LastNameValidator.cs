using Api.Controllers.Core;
using Api.Domain.Core;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Api.Features.Core;

public class LastNameValidator : AbstractValidator<LastName>
{
    public LastNameValidator(IStringLocalizer<FamilyMemberController> stringLocalizer)
    {
        RuleFor(n => n.Value)
            .NotNull()
            .WithMessage(stringLocalizer["First name can not be empty."])
            .NotEmpty()
            .WithMessage(stringLocalizer["First name can not be empty."]);
    }
}