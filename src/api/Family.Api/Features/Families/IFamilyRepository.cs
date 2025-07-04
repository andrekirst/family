using Family.Api.Features.Families.Models;

namespace Family.Api.Features.Families;

public interface IFamilyRepository
{
    Task<Models.Family?> GetByIdAsync(string familyId, CancellationToken cancellationToken = default);
    Task<Models.Family?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<Models.Family?> GetByMemberIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SaveAsync(Models.Family family, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string familyId, CancellationToken cancellationToken = default);
}