namespace Api.Domain.Core;

public class LastNameRequiredException(string? value) : Exception
{
    public string? Value { get; } = value;
}