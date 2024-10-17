namespace Api.Domain.Body;

public class WeightIsTooLowException : Exception
{
    public double Value { get; }

    public WeightIsTooLowException(double value)
    {
        Value = value;
    }
}