using System.ComponentModel.DataAnnotations.Schema;
using Api.Domain.Core;

namespace Api.Infrastructure;

public class LabelBaseRelationEntity<TReferenceTable> : BaseEntity
{
    public Guid LabelId { get; set; }
    
    [ForeignKey(nameof(LabelId))]
    public Label Label { get; set; } = default!;

    public Guid ReferenceTableId { get; set; }
    
    [ForeignKey(nameof(ReferenceTableId))]
    public TReferenceTable ReferenceTable { get; set; } = default!;
}