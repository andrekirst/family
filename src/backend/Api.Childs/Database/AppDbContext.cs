using Api.Childs.Database.Models;
using Api.Childs.Infrastructure;
using Api.Childs.Services;
using Microsoft.EntityFrameworkCore;

namespace Api.Childs.Database;

public class AppDbContext : DbContext
{
    private readonly IDateTimeProvider? _dateTimeProvider;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public AppDbContext(
        IDateTimeProvider dateTimeProvider,
        IHttpContextAccessor httpContextAccessor,
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
        _dateTimeProvider = dateTimeProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<Child> Childs => Set<Child>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var newEntities = ChangeTracker.Entries()
            .Where(entry => entry.State == EntityState.Added && entry.Entity is Entity)
            .Select(entry => entry.Entity as Entity);

        var modifiedEntities = ChangeTracker.Entries()
            .Where(entry => entry.State == EntityState.Modified && entry.Entity is Entity)
            .Select(entry => entry.Entity as Entity);

        var currentTime = _dateTimeProvider?.Now ?? DateTime.Now;
        var username = _httpContextAccessor?.HttpContext?.User.Identity?.Name ?? "Unknown user";

        foreach (var newEntity in newEntities)
        {
            if (newEntity == null)
            {
                continue;
            }

            newEntity.CreatedAt = currentTime;
            newEntity.CreatedBy = username;
            newEntity.ModifiedAt = currentTime;
            newEntity.ModifiedBy = username;
        }

        foreach (var modifiedEntity in modifiedEntities)
        {
            if (modifiedEntity == null)
            {
                continue;
            }

            modifiedEntity.ModifiedAt = currentTime;
            modifiedEntity.ModifiedBy = username;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}