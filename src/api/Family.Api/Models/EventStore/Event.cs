using Family.Api.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace Family.Api.Models.EventStore;

public class Event : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string AggregateId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string AggregateType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string EventType { get; set; } = string.Empty;
    
    [Required]
    public string EventData { get; set; } = string.Empty;
    
    [Required]
    public string Metadata { get; set; } = "{}";
    
    [Required]
    public int Version { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; }
    
    [Required]
    [StringLength(100)]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string CorrelationId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string CausationId { get; set; } = string.Empty;
}