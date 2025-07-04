using Family.Api.Models.Base;

namespace Family.Api.Services.EventStore;

public interface IEventSourcedRepository<T> where T : AggregateRoot, new()
{
    Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    
    Task<T?> GetByIdAsync(string id, int version, CancellationToken cancellationToken = default);
    
    Task<T> GetByIdAsync(string id, DateTime timestamp, CancellationToken cancellationToken = default);
    
    Task SaveAsync(T aggregate, CancellationToken cancellationToken = default);
    
    Task SaveAsync(T aggregate, int expectedVersion, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<T>> GetAllAsync(int pageSize = 100, int pageNumber = 1, CancellationToken cancellationToken = default);
    
    Task<T> ReplayToVersionAsync(string id, int version, CancellationToken cancellationToken = default);
    
    Task<T> ReplayToTimestampAsync(string id, DateTime timestamp, CancellationToken cancellationToken = default);
}