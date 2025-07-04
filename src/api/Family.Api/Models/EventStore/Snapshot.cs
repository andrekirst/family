using Family.Api.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace Family.Api.Models.EventStore;

public class Snapshot : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string AggregateId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string AggregateType { get; set; } = string.Empty;
    
    [Required]
    public string Data { get; set; } = string.Empty;
    
    [Required]
    public int Version { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; }
}