using Api.Childs.Database.Models;
using Api.Childs.Infrastructure;

namespace Api.Childs.Database.Repositories;

public interface IChildRepository : IRepository<Child>
{
}

public class ChildRepository : BaseRepository<Child>, IChildRepository
{
    public ChildRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}