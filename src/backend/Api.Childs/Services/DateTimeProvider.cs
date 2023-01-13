namespace Api.Childs.Services;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}

public static class DateTimeProviderServiceRegistrations
{
    public static IServiceCollection AddDateTimeProvider(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        return services;
    }
}