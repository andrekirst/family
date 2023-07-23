using Api.Controllers.Core;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Api.Domain.Core;

public class FirstNameValidator : AbstractValidator<FirstName>
{
    public FirstNameValidator(IStringLocalizer<FamilyMemberController> stringLocalizer)
    {
        RuleFor(_ => _.Value)
            .NotNull()
            .WithMessage(stringLocalizer["First name can not be empty."])
            .NotEmpty()
            .WithMessage(stringLocalizer["First name can not be empty."])
            .OverridePropertyName(string.Empty);
    }
}