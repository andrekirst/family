using Family.Infrastructure.Messaging.Abstractions;
using Family.Infrastructure.Messaging.Configuration;
using Family.Infrastructure.Messaging.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace Family.Infrastructure.Messaging.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Kafka settings
        services.Configure<KafkaConfiguration>(options =>
            configuration.GetSection(KafkaConfiguration.SectionName).Bind(options));

        // Register services
        services.AddSingleton<IEventSerializer, EventSerializer>();
        services.AddSingleton<IEventBusPublisher, KafkaEventBusPublisher>();
        services.AddSingleton<IEventBusConsumer, KafkaEventBusConsumer>();
        
        // Register consumer as hosted service
        services.AddSingleton<IHostedService>(provider => 
            (KafkaEventBusConsumer)provider.GetRequiredService<IEventBusConsumer>());

        return services;
    }

    public static IServiceCollection AddKafkaMessaging(
        this IServiceCollection services,
        Action<KafkaConfiguration> configureOptions)
    {
        services.Configure(configureOptions);

        // Register services
        services.AddSingleton<IEventSerializer, EventSerializer>();
        services.AddSingleton<IEventBusPublisher, KafkaEventBusPublisher>();
        services.AddSingleton<IEventBusConsumer, KafkaEventBusConsumer>();
        
        // Register consumer as hosted service
        services.AddSingleton<IHostedService>(provider => 
            (KafkaEventBusConsumer)provider.GetRequiredService<IEventBusConsumer>());

        return services;
    }
}