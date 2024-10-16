
namespace Api.Infrastructure;

public class Result<TValue> : Result<TValue, Error>
{
    private Result(TValue value) : base(value)
    {
    }

    private Result(Error error) : base(error)
    {
    }
    
    public static implicit operator Result<TValue>(TValue value) => new(value);
    public static implicit operator Result<TValue>(Error error) => new(error);

    public new Result<TValue, Error> Match(Func<TValue, Result<TValue, Error>> success, Func<Error, Result<TValue, Error>> failure)
        => IsSuccess ? success(Value!) : failure(Error!);
}

public class Result<TValue, TError>
{
    public TValue? Value { get; private init; }
    public TError? Error { get; private init; }
    public bool IsSuccess { get; private init; }
    public bool IsError => !IsSuccess;
    
    protected Result(TValue value)
    {
        IsSuccess = true;
        Value = value;
        Error = default;
    }

    protected Result(TError error)
    {
        IsSuccess = false;
        Value = default;
        Error = error;
    }

    public static implicit operator Result<TValue, TError>(TValue value) => new(value);
    public static implicit operator Result<TValue, TError>(TError error) => new(error);

    public Result<TValue, TError> Match(Func<TValue, Result<TValue, TError>> success, Func<TError, Result<TValue, TError>> failure)
        => IsSuccess ? success(Value!) : failure(Error!);
}

public sealed record Error(string Code, string? Message = null);