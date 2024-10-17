using Api.Domain.Core;

namespace Api.Infrastructure.Database;

public interface IHasLabelsEntity
{
    IEnumerable<LabelEntity> Labels { get; set; }
}