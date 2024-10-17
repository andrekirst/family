namespace Api.Infrastructure.Database;

public interface IEntityRowVersion
{
    string? RowVersion { get; set; }
}