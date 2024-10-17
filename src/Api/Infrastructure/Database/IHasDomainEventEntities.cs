using Api.Database.Core;

namespace Api.Infrastructure.Database;

public interface IHasDomainEventEntities
{
    ICollection<DomainEventEntity> CreatedByDomainEventEntries { get; set; }
    ICollection<DomainEventEntity> CreatedForDomainEventEntries { get; set; }
}