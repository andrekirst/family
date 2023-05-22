using FluentValidation;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using WebUi.Data;
using WebUi.Validation;

[assembly: AspMvcAreaViewLocationFormat("/Areas/{2}/{1}/{0}/View.cshtml")]
[assembly: AspMvcViewLocationFormat("/Views/{1}/{0}/View.cshtml")]
[assembly: AspMvcViewLocationFormat("/Views/{0}/{1}.cshtml")]

namespace WebUi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("IdentityDbConnection") ?? throw new InvalidOperationException("Connection string 'IdentityDbConnection' not found.");
                options.UseSqlServer(connectionString);
            });
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services
                .AddDefaultIdentity<IdentityUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;
                    options.SignIn.RequireConfirmedEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddAuthentication()
                .AddCookie(options =>
                {
                    options.LoginPath = "/account/google-login";
                })
                .AddGoogle(options =>
                {
                    var config = builder.Configuration.GetSection("Security:Authentication:Google");
                    if (!config.Exists())
                    {
                        return;
                    }
                    var clientId = config.GetValue<string>("ClientId");
                    var clientSecret = config.GetValue<string>("ClientSecret");
                    ArgumentException.ThrowIfNullOrEmpty(clientId, nameof(clientId));
                    ArgumentException.ThrowIfNullOrEmpty(clientSecret, nameof(clientSecret));
                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                });

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                const string defaultCulture = "de";
                var supportedCultures = new[] { "en", defaultCulture };
                options
                    .SetDefaultCulture(defaultCulture)
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures);

                options.AddInitialRequestCultureProvider(new CookieRequestCultureProvider
                {
                    CookieName = "FamilyLanguage"
                });
            });
            builder.Services
                .AddControllersWithViews()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.SubFolder)
                .AddDataAnnotationsLocalization();
            
            builder.Services.Configure<RazorViewEngineOptions>(options =>
            {
                options.AreaViewLocationFormats.Insert(0, "/Areas/{2}/{1}/{0}/View" + RazorViewEngine.ViewExtension);
                options.ViewLocationFormats.Insert(0, "/Views/{1}/{0}/View" + RazorViewEngine.ViewExtension);
                options.PageViewLocationFormats.Insert(0, "Areas/{1}/{0}/Page" + RazorViewEngine.ViewExtension);
                options.AreaPageViewLocationFormats.Insert(0, "/Areas/{2}/{1}/{0}/Page" + RazorViewEngine.ViewExtension);
            });
            builder.Services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<ISystemClock, SystemClock>();
            builder.Services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssemblyContaining<Program>();
            });

            builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            var currentAssembly = typeof(Program).Assembly;
            builder.Services.AddValidatorsFromAssembly(currentAssembly);
            builder.Services.AddAutoMapper(currentAssembly);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthorization();
            app.UseRequestLocalization();

            app.MapControllerRoute(
                    name: Routes.AreaRouteName,
                    pattern: Routes.AreaRoutePattern)
                .RequireAuthorization();

            app
                .MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
                .RequireAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}