namespace ui.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia! :-)";
    public string LoginWithGoogle => "Login with Google";
#pragma warning restore CA1822 // Mark members as static
}
