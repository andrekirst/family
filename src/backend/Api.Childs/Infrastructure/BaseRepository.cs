using Api.Childs.Database;
using Microsoft.EntityFrameworkCore;

namespace Api.Childs.Infrastructure;

public abstract class BaseRepository<TEntity> : IRepository<TEntity>
    where TEntity : class
{
    protected BaseRepository(AppDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public AppDbContext DbContext { get; }
    public DbSet<TEntity> DbSet { get; }

    public virtual void Update(TEntity entity) => DbSet.Update(entity);
    public virtual void UpdateRange(IEnumerable<TEntity> entities) => DbSet.UpdateRange(entities);
    public virtual void Add(TEntity entity) => DbSet.Add(entity);
    public virtual void AddRange(IEnumerable<TEntity> entities) => DbSet.AddRange(entities);
    public virtual void RemoveRange(IEnumerable<TEntity> entities) => DbSet.RemoveRange(entities);
    public virtual void Remove(TEntity entity) => DbSet.Remove(entity);
}