using Api.Infrastructure;

namespace Api.Domain.Core;

public record LastName : IValueObjectFrom<LastName, string>
{
    public string Value { get; init; }

    public LastName(string? value)
        : this(value, true)
    {
    }

    private LastName(string? value, bool executeValidation)
    {
        if (executeValidation && string.IsNullOrWhiteSpace(value))
        {
            throw new LastNameRequiredException(value);
        }

        Value = value!;
    }

    public static LastName From(string value) => new LastName(value);
    public static LastName FromNull(string? value) => new LastName(value);

    public static LastName FromRaw(string value) => new LastName(value, false);
    public static LastName FromNullRaw(string? value) => new LastName(value, false);
}