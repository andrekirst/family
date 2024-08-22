using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using ui.Extensions;
using ui.ViewModels;
using ui.Views;

namespace ui;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        BindingPlugins.DataValidators.RemoveAt(0);

        var collection = new ServiceCollection();
        collection.RegisterServices();

        var services = collection.BuildServiceProvider();

        var mainWindowViewModel = services.GetRequiredService<MainWindowViewModel>();
        
        switch (ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };
                break;
            case ISingleViewApplicationLifetime singleViewApplicationLifetime:
                singleViewApplicationLifetime.MainView = new MainWindow
                {
                    DataContext = mainWindowViewModel
                };
                break;
        }

        base.OnFrameworkInitializationCompleted();
    }
}