namespace Api.Infrastructure;

public interface IHasSuccessAndHasError
{
    bool IsSuccess { get; }
    bool IsError { get; }
}