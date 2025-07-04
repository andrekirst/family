using Family.Api.Services.EventStore;

namespace Family.Api.Extensions;

public static class EventStoreExtensions
{
    public static IServiceCollection AddEventStore(this IServiceCollection services)
    {
        services.AddScoped<IEventStore, EventStore>();
        services.AddScoped<IEventReplayService, EventReplayService>();
        services.AddScoped(typeof(IEventSourcedRepository<>), typeof(EventSourcedRepository<>));
        
        return services;
    }
}