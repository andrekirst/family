using Family.Api.Data;
using Family.Api.Data.Entities;
using Family.Infrastructure.EventSourcing.Services;
using Microsoft.EntityFrameworkCore;

namespace Family.Api.Features.Families;

public class FamilyRepository : IFamilyRepository
{
    private readonly IEventSourcedRepository<Models.Family> _eventSourcedRepository;
    private readonly IEventStore _eventStore;
    private readonly FamilyDbContext _dbContext;

    public FamilyRepository(
        IEventSourcedRepository<Models.Family> eventSourcedRepository,
        IEventStore eventStore,
        FamilyDbContext dbContext)
    {
        _eventSourcedRepository = eventSourcedRepository;
        _eventStore = eventStore;
        _dbContext = dbContext;
    }

    public async Task<Models.Family?> GetByIdAsync(string familyId, CancellationToken cancellationToken = default)
    {
        return await _eventSourcedRepository.GetByIdAsync(familyId, cancellationToken);
    }

    public async Task<Models.Family?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        // Use read model for better performance
        var familyEntity = await _dbContext.Families
            .Include(f => f.Members)
            .FirstOrDefaultAsync(f => f.OwnerId == ownerId, cancellationToken);

        if (familyEntity == null)
            return null;

        // Convert to domain model and replay events to ensure consistency
        return await _eventSourcedRepository.GetByIdAsync(familyEntity.Id, cancellationToken);
    }

    public async Task<Models.Family?> GetByMemberIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Use read model for better performance
        var familyMember = await _dbContext.FamilyMembers
            .Include(fm => fm.Family)
            .ThenInclude(f => f!.Members)
            .FirstOrDefaultAsync(fm => fm.UserId == userId, cancellationToken);

        if (familyMember?.Family == null)
            return null;

        // Convert to domain model and replay events to ensure consistency
        return await _eventSourcedRepository.GetByIdAsync(familyMember.FamilyId, cancellationToken);
    }

    public async Task SaveAsync(Models.Family family, CancellationToken cancellationToken = default)
    {
        // Save events using Event Sourcing
        // Event handlers will automatically update read models via MediatR notifications
        await _eventSourcedRepository.SaveAsync(family, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string familyId, CancellationToken cancellationToken = default)
    {
        // Use read model for better performance
        return await _dbContext.Families.AnyAsync(f => f.Id == familyId, cancellationToken);
    }
}