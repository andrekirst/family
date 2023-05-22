using System.ComponentModel.DataAnnotations;

namespace WebUi.Data.Model;

public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; }
    
    [MaxLength(Lengths.TextField)]
    public string? CreatedBy { get; set; }
    public DateTime ChangedAt { get; set; }

    [MaxLength(Lengths.TextField)]
    public string? ChangedBy { get; set; }
}