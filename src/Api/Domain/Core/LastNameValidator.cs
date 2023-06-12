using Api.Controllers;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Api.Domain.Core;

public class LastNameValidator : AbstractValidator<LastName>
{
    public LastNameValidator(IStringLocalizer<FamilyMemberController> stringLocalizer)
    {
        RuleFor(_ => _.Value)
            .NotNull()
            .WithMessage(stringLocalizer["First name can not be empty."])
            .NotEmpty()
            .WithMessage(stringLocalizer["First name can not be empty."]);
    }
}