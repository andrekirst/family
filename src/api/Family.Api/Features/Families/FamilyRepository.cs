using Family.Infrastructure.EventSourcing.Services;

namespace Family.Api.Features.Families;

public class FamilyRepository : IFamilyRepository
{
    private readonly IEventSourcedRepository<Models.Family> _eventSourcedRepository;
    private readonly IEventStore _eventStore;

    public FamilyRepository(
        IEventSourcedRepository<Models.Family> eventSourcedRepository,
        IEventStore eventStore)
    {
        _eventSourcedRepository = eventSourcedRepository;
        _eventStore = eventStore;
    }

    public async Task<Models.Family?> GetByIdAsync(string familyId, CancellationToken cancellationToken = default)
    {
        return await _eventSourcedRepository.GetByIdAsync(familyId, cancellationToken);
    }

    public async Task<Models.Family?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        // Get family created events by event type
        var events = await _eventStore.GetEventsByTypeAsync(nameof(DomainEvents.FamilyCreated), cancellationToken: cancellationToken);
        
        foreach (var eventEntity in events)
        {
            var domainEvent = eventEntity as DomainEvents.FamilyCreated;
            if (domainEvent?.OwnerId == ownerId)
            {
                return await GetByIdAsync(domainEvent.AggregateId, cancellationToken);
            }
        }

        return null;
    }

    public async Task<Models.Family?> GetByMemberIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Get family member added events by event type
        var events = await _eventStore.GetEventsByTypeAsync(nameof(DomainEvents.FamilyMemberAdded), cancellationToken: cancellationToken);
        
        foreach (var eventEntity in events)
        {
            var domainEvent = eventEntity as DomainEvents.FamilyMemberAdded;
            if (domainEvent?.MemberUserId == userId)
            {
                return await GetByIdAsync(domainEvent.AggregateId, cancellationToken);
            }
        }

        return null;
    }

    public async Task SaveAsync(Models.Family family, CancellationToken cancellationToken = default)
    {
        await _eventSourcedRepository.SaveAsync(family, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string familyId, CancellationToken cancellationToken = default)
    {
        var family = await GetByIdAsync(familyId, cancellationToken);
        return family != null;
    }
}