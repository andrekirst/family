namespace Family.Infrastructure.CQRS.Abstractions;

/// <summary>
/// Represents the result of a command execution
/// </summary>
public class CommandResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public Dictionary<string, string[]>? ValidationErrors { get; init; }
    
    public static CommandResult Success() => new() { IsSuccess = true };
    
    public static CommandResult Failure(string errorMessage) => new() 
    { 
        IsSuccess = false, 
        ErrorMessage = errorMessage 
    };
    
    public static CommandResult ValidationFailure(Dictionary<string, string[]> validationErrors) => new()
    {
        IsSuccess = false,
        ValidationErrors = validationErrors
    };
}

/// <summary>
/// Represents the result of a command execution with data
/// </summary>
/// <typeparam name="T">The type of data returned</typeparam>
public class CommandResult<T> : CommandResult
{
    public T? Data { get; init; }
    
    public static CommandResult<T> Success(T data) => new() 
    { 
        IsSuccess = true, 
        Data = data 
    };
    
    public static new CommandResult<T> Failure(string errorMessage) => new() 
    { 
        IsSuccess = false, 
        ErrorMessage = errorMessage 
    };
    
    public static new CommandResult<T> ValidationFailure(Dictionary<string, string[]> validationErrors) => new()
    {
        IsSuccess = false,
        ValidationErrors = validationErrors
    };
}