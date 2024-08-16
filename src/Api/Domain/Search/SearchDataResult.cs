namespace Api.Domain.Search;

public record SearchDataResult
{
    public ObjectType ObjectType { get; set; }
    public string Title { get; set; } = default!;
    public Guid ValueId { get; set; }
}