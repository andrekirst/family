using Family.Infrastructure.EventSourcing.Data;
using Family.Infrastructure.EventSourcing.Services;

namespace Family.Infrastructure.EventSourcing.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventSourcing(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Event Sourcing DbContext
        var connectionString = configuration.GetConnectionString("EventSourcingConnection") 
            ?? configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=family_events;Username=family;Password=family";

        services.AddDbContext<EventSourcingDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register Event Store services
        services.AddScoped<IEventStore, EventStore>();
        services.AddScoped<IEventReplayService, EventReplayService>();
        services.AddScoped(typeof(IEventSourcedRepository<>), typeof(EventSourcedRepository<>));

        return services;
    }
}