namespace Api.Domain.Body.WeightTracking;

public class WeightIsTooLowException : Exception
{
    public double Value { get; }

    public WeightIsTooLowException(double value)
    {
        Value = value;
    }
}