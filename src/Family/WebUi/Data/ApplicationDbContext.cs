using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using WebUi.Data.Model;

namespace WebUi.Data;

public class ApplicationDbContext : IdentityDbContext
{
    private readonly ISystemClock? _systemClock;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public ApplicationDbContext(
        ISystemClock systemClock,
        IHttpContextAccessor httpContextAccessor,
        DbContextOptions<ApplicationDbContext> options)
        : this(options)
    {
        _systemClock = systemClock;
        _httpContextAccessor = httpContextAccessor;
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {


        return base.SaveChangesAsync(cancellationToken);
    }
}