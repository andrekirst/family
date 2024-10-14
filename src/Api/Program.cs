using System.Text;
using System.Text.Json.Serialization;
using Api.Database;
using Api.Domain;
using Api.Domain.Core;
using Api.Features.Core;
using Api.Infrastructure;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using ISystemClock = Microsoft.Extensions.Internal.ISystemClock;
using SystemClock = Microsoft.Extensions.Internal.SystemClock;

// TODO
// https://medium.com/geekculture/how-to-add-jwt-authentication-to-an-asp-net-core-api-84e469e9f019

namespace Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        IdentityModelEventSource.ShowPII = true;

        builder.Services
            .AddControllers(options =>
            {
                options.Filters.Add(new AuthorizeFilter());
                if (!builder.Environment.IsDevelopment())
                {
                    options.Filters.Add(new RequireHttpsAttribute());   
                }
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.AddHttpClient();
        builder.Services.UseGoogleAuthenticationOptions();
        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var googleClientId = builder.Configuration.GetSection("Authentication:Google:ClientId").Value;
                
                options.Authority = "https://accounts.google.com"; // Google als Token-Issuer
                options.Audience = googleClientId; // Dein Google Client ID hier
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        return Task.CompletedTask;
                    } 
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "https://accounts.google.com", // Google Issuer
                    ValidateAudience = true,
                    ValidAudience = googleClientId, // Dein Google Client ID hier
                    ValidateLifetime = true, // Token Ablaufzeit validieren
                    ValidateIssuerSigningKey = true, // Signatur des Tokens validieren
                }; 
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Family.Api", Version = "v1"});
        });
        builder.Services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblyContaining<Program>();
            configuration.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>), ServiceLifetime.Scoped);
        });

        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        builder.Services.AddLocalization();

        var defaultConnectionString = builder.Configuration.GetConnectionString("Default");
        
        builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
        builder.Services.AddDbContext<ApplicationDbContext>(optionsBuilder =>
        {
            var isNotProduction = !builder.Environment.IsProduction();
            
            optionsBuilder
                .UseNpgsql(builder.Configuration.GetConnectionString("Default"))
                .EnableDetailedErrors(isNotProduction)
                .EnableSensitiveDataLogging(isNotProduction)
                .EnableServiceProviderCaching();
        });

        builder.Services.AddDbContext<UsersContext>(optionsBuilder =>
        {
            var isNotProduction = !builder.Environment.IsProduction();

            optionsBuilder
                .UseNpgsql(builder.Configuration.GetConnectionString("Default"))
                .EnableDetailedErrors(isNotProduction)
                .EnableSensitiveDataLogging(isNotProduction)
                .EnableServiceProviderCaching();
        });
        builder.Services
            .AddIdentityCore<IdentityUser>(options =>
            {
                var isProduction = builder.Environment.IsProduction();
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = isProduction;
                options.Password.RequiredLength = isProduction ? 8 : 4;
                options.Password.RequireNonAlphanumeric = isProduction;
                options.Password.RequireUppercase = isProduction;
                options.Password.RequireLowercase = isProduction;
            })
            .AddEntityFrameworkStores<UsersContext>();

        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<DomainEventRepository>();
        builder.Services.AddScoped<CurrentFamilyMemberIdService>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddAutoMapper(configure =>
        {
            configure.AddMaps(typeof(GetFamilyMembersQueryMappings));
        });

        builder.Services.AddUnitOfWork();
        builder.Services.AddTransient<ExceptionHandlingMiddleware>();
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policyBuilder =>
                policyBuilder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin());
        });

        builder.Services.AddSingleton<ISystemClock, SystemClock>();
        builder.Services.AddDomainServices();

        var app = builder.Build();

        //app.UseRequestLocalization(options =>
        //{
        //    options.ApplyCurrentCultureToResponseHeaders = true;
        //    // TODO
        //    options.DefaultRequestCulture = new RequestCulture("de-DE");
        //});

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        if (!builder.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();   
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseCors();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.MapControllers();

        await MigrateDbContext(app);

        await app.RunAsync();
    }

    private static async Task MigrateDbContext(IHost app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        var usersContext = scope.ServiceProvider.GetRequiredService<UsersContext>();
        await usersContext.Database.MigrateAsync();
    }
}