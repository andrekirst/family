using Api.Domain.Body;
using FluentValidation;

namespace Api.Features.Body;

public class WeightValidator : AbstractValidator<Weight>
{
    public WeightValidator()
    {
        RuleFor(w => w.Value)
            .NotNull()
            .GreaterThan(Weight.MinimalWeight);
    }
}