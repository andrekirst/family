using Api.Infrastructure;

namespace Api.Domain.Core;

public record Birthdate : IValueObjectFrom<Birthdate, DateTime>
{
    public DateTime Value { get; init; }

    public Birthdate(DateTime? value, bool executeValidation = true)
    {
        Value = executeValidation switch
        {
            true when value == null => throw new BirthdateRequiredException(),
            true when value == DateTime.MinValue => throw new BirthdateTooEarlyException(value),
            _ => value!.Value
        };
    }

    public static Birthdate From(DateTime value) => new Birthdate(value);

    public static Birthdate FromNull(DateTime value) => new Birthdate(value);

    public static Birthdate FromRaw(DateTime value) => new Birthdate(value, false);

    public static Birthdate FromNullRaw(DateTime value) => new Birthdate(value, false);
}