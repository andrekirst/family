using Api.Domain.Core;

namespace Api.Infrastructure;

public interface IHasLabels
{
    IEnumerable<LabelEntity> Labels { get; set; }
}