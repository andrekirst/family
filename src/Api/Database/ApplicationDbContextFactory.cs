using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Api.Database;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("Default"));
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}