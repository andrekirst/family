namespace Api.Domain.Core;

public class BirthdateTooEarlyException : Exception
{
    public DateTime? Value { get; }

    public BirthdateTooEarlyException(DateTime? value)
    {
        Value = value;
    }
}