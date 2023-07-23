using Api.Domain.Core;

namespace Api.Infrastructure;

public interface IHasLabels
{
    IEnumerable<Label> Labels { get; set; }
}