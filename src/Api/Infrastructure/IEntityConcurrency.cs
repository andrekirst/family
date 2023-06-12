namespace Api.Infrastructure;

public interface IEntityConcurrency
{
    string? ConcurrencyToken { get; set; }
}