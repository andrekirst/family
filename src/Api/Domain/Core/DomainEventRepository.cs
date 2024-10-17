using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Database;
using Api.Database.Core;
using Api.Infrastructure;
using Api.Infrastructure.DomainEvents;

namespace Api.Domain.Core;

public class DomainEventRepository(
    ApplicationDbContext dbContext,
    IUnitOfWork unitOfWork)
{
    public async Task AddAsync<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
        where TDomainEvent : IDomainEvent
    {
        var json = JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        });

        var attribute = domainEvent.GetDomainEventAttribute();
        
        var entry = new DomainEventEntity
        {
            EventType = attribute.Name,
            EventVersion = attribute.Version,
            EventData = json
        };

        dbContext.DomainEventEntries.Add(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}