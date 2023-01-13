namespace Api.Childs.Infrastructure;

public interface IRepository<in TEntity>
    where TEntity : class
{
    void Update(TEntity entity);
    void UpdateRange(IEnumerable<TEntity> entities);
    void Add(TEntity entity);
    void AddRange(IEnumerable<TEntity> entities);
    void RemoveRange(IEnumerable<TEntity> entities);
    void Remove(TEntity entity);
}