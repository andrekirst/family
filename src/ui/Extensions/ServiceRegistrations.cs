using Microsoft.Extensions.DependencyInjection;
using ui.ViewModels;

namespace ui.Extensions;

public static class ServiceRegistrations
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddTransient<MainWindowViewModel>();
    }
}