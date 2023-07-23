using Api.Infrastructure;

namespace Api.Domain.Body.WeightTracking;

public class Weight : IValueObjectFrom<Weight, double>
{
    public const double MinimalWeight = 0;

    public double? Value { get; }

    public Weight(double? value)
        : this(value, true)
    {
        Value = value;
    }

    public Weight(double? value, bool executeValidation)
    {
        if (executeValidation)
        {
            switch (value)
            {
                case null:
                    throw new WeightIsRequiredException();
                case <= MinimalWeight:
                    throw new WeightIsTooLowException(value.GetValueOrDefault());
            }
        }

        Value = value;
    }

    public static Weight From(double value) => new Weight(value);

    public static Weight FromNull(double value) => new Weight(value);

    public static Weight FromRaw(double value) => new Weight(value, false);

    public static Weight FromNullRaw(double value) => new Weight(value, false);
}