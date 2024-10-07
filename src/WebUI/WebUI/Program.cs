using System.IO.Compression;
using Family.Libraries.AspNet.Mvc.Middlewares;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using WebUI.Areas;
using WebUI.Modules;

namespace WebUI;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services
            .AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder(AuthenticationModule.ZitadelAuthenticationSchema)
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        builder.Services
            .Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.SmallestSize;
            })
            .Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.SmallestSize;
            });

        builder.Services.AddResponseCaching();
        
        builder.AddAppAuthentication();

        var app = builder.Build();

        // app.UseCors();

        app.UseResponseCaching();
        app.UseResponseCompression();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/App/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();
        app.UseAuthentication();

        app.UseRedirectToWhen("/", "/App");
        
        app.MapAreaControllerRoute(
            name: "AppArea",
            areaName: AreaNames.App,
            pattern: "App/{controller=Home}/{action=Index}/{id?}");
        
        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
        
        app.MapControllerRoute(
            name: "default",
            pattern: "App/{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}