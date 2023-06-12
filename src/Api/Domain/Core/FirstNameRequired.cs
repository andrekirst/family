namespace Api.Domain.Core;

public class FirstNameRequiredException : Exception
{
    public string? Value { get; }

    public FirstNameRequiredException(string? value)
    {
        Value = value;
    }
}