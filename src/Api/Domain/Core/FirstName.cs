using Api.Infrastructure;

namespace Api.Domain.Core;

public record FirstName : IValueObjectFrom<FirstName, string>
{
    public string Value { get; init; }

    public FirstName(string? value)
        : this(value, true)
    {
    }

    private FirstName(string? value, bool executeValidation)
    {
        if (executeValidation)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new FirstNameRequiredException(value);
            }
        }

        Value = value!;
    }

    public static FirstName From(string value) => new FirstName(value);
    public static FirstName FromNull(string? value) => new FirstName(value);
    public static FirstName FromRaw(string value) => new FirstName(value, false);
    public static FirstName FromNullRaw(string? value) => new FirstName(value, false);
}