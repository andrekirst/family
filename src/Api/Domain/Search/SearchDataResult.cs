namespace Api.Domain.Search;

public record SearchDataResult
{
    public ObjectType ObjectType { get; set; }
    public string Title { get; set; } = default!;
    public int ValueId { get; set; }
}