namespace Api.Domain.Core;

public class LastNameRequiredException : Exception
{
    public string? Value { get; }

    public LastNameRequiredException(string? value)
    {
        Value = value;
    }
}