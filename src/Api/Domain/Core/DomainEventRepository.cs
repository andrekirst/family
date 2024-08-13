using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Database;
using Api.Infrastructure;

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

        var entry = new DomainEventEntry
        {
            EventType = domainEvent.DomainEventName,
            EventVersion = domainEvent.DomainEventVersion,
            EventData = json
        };

        dbContext.DomainEventEntries.Add(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}