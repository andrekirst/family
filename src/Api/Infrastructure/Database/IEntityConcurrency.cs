namespace Api.Infrastructure.Database;

public interface IEntityConcurrency
{
    string? ConcurrencyToken { get; set; }
}