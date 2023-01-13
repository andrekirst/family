using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Api.Childs.Database;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = GetConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }

    private static string GetConnectionString()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();
        return config.GetConnectionString("Default") ?? throw new ConnectionStringNotFoundException();
    }
}

internal class ConnectionStringNotFoundException : Exception
{
}