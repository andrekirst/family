namespace Api.Domain.Core;

public class FirstNameRequiredException(string? value) : Exception
{
    public string? Value { get; } = value;
}