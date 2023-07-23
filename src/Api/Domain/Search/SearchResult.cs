namespace Api.Domain.Search;

public record SearchResult
{
    public int ValueId { get; set; }
    public string Title { get; set; } = default!;
    public ObjectType ObjectType { get; set; }
}