using Api.Controllers;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Api.Domain.Core;

public class BirthdateValidator : AbstractValidator<Birthdate>
{
    public BirthdateValidator(IStringLocalizer<FamilyMemberController> stringLocalizer)
    {
        RuleFor(_ => _.Value)
            .GreaterThan(DateTime.MinValue)
            .WithMessage(stringLocalizer["Birth date must be a correct date"]);
    }
}