namespace Api.Infrastructure.Database;

public interface IEntityMetadata
{
    public string? CreatedBy { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public string? ChangedBy { get; set; }
    public DateTimeOffset? ChangedAt { get; set; }
}