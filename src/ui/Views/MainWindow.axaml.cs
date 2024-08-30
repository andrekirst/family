using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ui.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void LoginWithGoogleClick(object? sender, RoutedEventArgs e)
    {
        var clientId = "925228679396-5e676kb9m15d9e5i6bmp4bmiki0c9bnq.apps.googleusercontent.com";
        var redirectUri = "https://localhost:7076/signin-google";
        
        var googleAuthUrl = $"https://accounts.google.com/o/oauth2/auth?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope=openid%20email%20profile";
        Process.Start(new ProcessStartInfo(googleAuthUrl) { UseShellExecute = true });
    }
}