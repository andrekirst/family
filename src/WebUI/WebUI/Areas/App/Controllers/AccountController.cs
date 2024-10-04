using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WebUI.Modules;

namespace WebUI.Areas.App.Controllers;

[Area(AreaNames.App)]
public class AccountController : Controller
{
    public IActionResult Login()
    {
        var authenticationProperties = new AuthenticationProperties
        {
            RedirectUri = Url.ActionLink("Index", "Home")
        };

        return Challenge(
            authenticationProperties,
            AuthenticationModule.ZitadelAuthenticationSchema);
    }
    
    public IActionResult Logout()
    {
        var authenticationProperties = new AuthenticationProperties
        {
            RedirectUri = Url.ActionLink("SignedOut")
        };
        
        return SignOut(
            authenticationProperties,
            CookieAuthenticationDefaults.AuthenticationScheme,
            AuthenticationModule.ZitadelAuthenticationSchema);
    }
    
    public IActionResult SignedOut()
    {
        return View();
    }
}