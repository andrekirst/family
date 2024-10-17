using Api.Database.Core;

namespace Api.Infrastructure;

public interface IHasDomainEventEntries
{
    ICollection<DomainEventEntity> CreatedByDomainEventEntries { get; set; }
    ICollection<DomainEventEntity> CreatedForDomainEventEntries { get; set; }
}