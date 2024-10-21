using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Database;
using Api.Database.Core;
using Api.Infrastructure;
using Api.Infrastructure.DomainEvents;

namespace Api.Domain.Core;

public interface IDomainEventRepository
{
    Task AddAsync<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
        where TDomainEvent : IDomainEvent;
}

public class DomainEventRepository(
    ApplicationDbContext dbContext,
    IUnitOfWork unitOfWork) : IDomainEventRepository
{
    public async Task AddAsync<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
        where TDomainEvent : IDomainEvent
    {
        var json = JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        });

        var attribute = domainEvent.GetDomainEventAttribute();
        
        var entity = new DomainEventEntity
        {
            EventType = attribute.Name,
            EventVersion = attribute.Version,
            EventData = json
        };

        dbContext.DomainEvents.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}