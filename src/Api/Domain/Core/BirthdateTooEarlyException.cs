namespace Api.Domain.Core;

public class BirthdateTooEarlyException(DateTime? value) : Exception
{
    public DateTime? Value { get; } = value;
}