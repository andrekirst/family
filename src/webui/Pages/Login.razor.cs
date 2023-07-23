using Microsoft.AspNetCore.Components;

namespace WebUI.Pages;

public partial class Login
{
    private readonly LoginRequestModel _loginRequestModel = new LoginRequestModel();

    public bool ShowAuthenticationError { get; set; }

    [Inject] public IAuthenticationService AuthenticationService { get; set; } = default!;

    [Inject] public NavigationManager NavigationManager { get; set; } = default!;

    public string? Error { get; set; }

    public async Task ExecuteLogin()
    {
        ShowAuthenticationError = false;
        var result = await AuthenticationService.Login(_loginRequestModel);
        if (!result.IsAuthenticationSuccessful)
        {
            Error = result.ErrorMessage;
            ShowAuthenticationError = true;
        }
        else
        {
            NavigationManager.NavigateTo("/");
        }
    }
}