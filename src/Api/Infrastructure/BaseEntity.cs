namespace Api.Infrastructure;

public abstract class BaseEntity : IEntityId, IEntityMetadata, IEntityConcurrency, IEntityRowVersion
{
    public Guid Id { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public string? ChangedBy { get; set; }
    public DateTimeOffset? ChangedAt { get; set; }
    public string? ConcurrencyToken { get; set; }
    public string? RowVersion { get; set; }
}