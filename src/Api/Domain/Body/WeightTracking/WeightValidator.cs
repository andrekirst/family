using FluentValidation;

namespace Api.Domain.Body.WeightTracking;

public class WeightValidator : AbstractValidator<Weight>
{
    public WeightValidator()
    {
        RuleFor(_ => _.Value)
            .NotNull()
            .GreaterThan(Weight.MinimalWeight);
    }
}