using Family.Infrastructure.CQRS.Behaviors;
using FluentValidation;
using System.Reflection;

namespace Family.Infrastructure.CQRS.Extensions;

/// <summary>
/// Extension methods for configuring CQRS services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CQRS services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan for handlers and validators</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCQRS(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Register MediatR
        services.AddMediatR(cfg =>
        {
            // Register handlers from specified assemblies
            foreach (var assembly in assemblies)
            {
                cfg.RegisterServicesFromAssembly(assembly);
            }
            
            // Add CQRS infrastructure assembly
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        });

        // Register pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Register FluentValidation
        foreach (var assembly in assemblies)
        {
            services.AddValidatorsFromAssembly(assembly);
        }

        return services;
    }

    /// <summary>
    /// Adds CQRS services with assembly scanning from calling assembly
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCQRS(this IServiceCollection services)
    {
        var callingAssembly = Assembly.GetCallingAssembly();
        return services.AddCQRS(callingAssembly);
    }
}