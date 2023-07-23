using Api.Domain.Core;

namespace Api.Infrastructure;

public interface IHasDomainEventEntries
{
    ICollection<DomainEventEntry> CreatedByDomainEventEntries { get; set; }
    ICollection<DomainEventEntry> CreatedForDomainEventEntries { get; set; }
}