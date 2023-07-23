using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Api.Database;

public class UsersContext : IdentityDbContext<IdentityUser>
{
    public UsersContext(DbContextOptions<UsersContext> options)
        : base(options)
    {
    }
}