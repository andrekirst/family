using Api.Infrastructure;

namespace Api.Domain.Core;

public record Birthdate : IValueObjectFrom<Birthdate, DateTime>
{
    public DateTime Value { get; init; }

    public Birthdate(DateTime? value)
        : this(value, true)
    {
    }

    public Birthdate(DateTime? value, bool executeValidation)
    {
        if (executeValidation)
        {
            if (value == null)
            {
                throw new BirthdateRequiredException();
            }

            if (value == DateTime.MinValue)
            {
                throw new BirthdateTooEarlyException(value);
            }
        }

        Value = value!.Value;
    }

    public static Birthdate From(DateTime value) => new Birthdate(value);

    public static Birthdate FromNull(DateTime value) => new Birthdate(value);

    public static Birthdate FromRaw(DateTime value) => new Birthdate(value, false);

    public static Birthdate FromNullRaw(DateTime value) => new Birthdate(value, false);
}

public class BirthdateTooEarlyException : Exception
{
    public DateTime? Value { get; }

    public BirthdateTooEarlyException(DateTime? value)
    {
        Value = value;
    }
}