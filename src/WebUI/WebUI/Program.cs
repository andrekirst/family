using Microsoft.AspNetCore.Mvc.Routing;
using WebUI.Areas;
using WebUI.Modules;

namespace WebUI;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        
        builder.AddAppAuthentication();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();
        app.UseAuthentication();
        
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/")
            {
                context.Response.Redirect("/App");
                return;
            }

            await next();
        });
        
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