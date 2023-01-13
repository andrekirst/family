namespace Api.Childs.Infrastructure;

public abstract class Entity
{
    public string CreatedBy { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}