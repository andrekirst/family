namespace Api.Infrastructure;

public interface IEntityRowVersion
{
    string? RowVersion { get; set; }
}