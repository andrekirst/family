using Api.Database.Authentication;
using Api.Database.Body;
using Api.Database.Calendar;
using Api.Database.Core;
using Api.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;

namespace Api.Database;

public class ApplicationDbContext : DbContext
{
    private readonly ISystemClock? _systemClock;
    private readonly HttpContext? _httpContext;

    public DbSet<FamilyMemberEntity> FamilyMembers => Set<FamilyMemberEntity>();
    public DbSet<WeightTrackingEntity> WeightTrackings => Set<WeightTrackingEntity>();
    public DbSet<DomainEventEntity> DomainEvents => Set<DomainEventEntity>();
    public DbSet<GoogleAccountEntity> GoogleAccounts => Set<GoogleAccountEntity>();
    public DbSet<CalendarEntity> Calendars => Set<CalendarEntity>();
    public DbSet<NotificationMessageEntity> NotificationMessages => Set<NotificationMessageEntity>();
    
    public ApplicationDbContext(
        ISystemClock systemClock,
        IHttpContextAccessor httpContextAccessor,
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
        _systemClock = systemClock;
        _httpContext = httpContextAccessor.HttpContext;
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Program).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // TODO Als Interceptor umbauen
        ChangeTracker.DetectChanges();

        foreach (var added in ChangeTracker.Entries()
                     .Where(e => e.State == EntityState.Added)
                     .Where(e => e.Entity is BaseEntity)
                     .Select(e => e.Entity as BaseEntity)
                     .Where(e => e != null))
        {
            added!.CreatedAt = _systemClock?.UtcNow ?? DateTimeOffset.UtcNow;
            added.CreatedBy = _httpContext?.User.Identity?.Name;
        }

        foreach (var modified in ChangeTracker.Entries()
                     .Where(e => e.State == EntityState.Modified)
                     .Where(e => e.Entity is BaseEntity)
                     .Select(e => e.Entity as BaseEntity)
                     .Where(e => e != null))
        {
            modified!.ChangedAt = _systemClock?.UtcNow ?? DateTimeOffset.UtcNow;
            modified.ChangedBy = _httpContext?.User.Identity?.Name;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}