using System.Text;
using System.Text.Json.Serialization;
using Api.Database;
using Api.Database.Body;
using Api.Database.Core;
using Api.Domain;
using Api.Domain.Core;
using Api.Features.Core;
using Api.Infrastructure;
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
                options.Filters.Add(new RequireHttpsAttribute());
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
            .AddCookie()
            // .AddGoogle(options =>
            // {
            //     var clientId = builder.Configuration["Authentication:Google:ClientId"];
            //     var clientSecrect = builder.Configuration["Authentication:Google:ClientSecret"];
            //     var redirectUri = builder.Configuration["Authentication:Google:RedirectUri"];
            //     
            //     ArgumentException.ThrowIfNullOrEmpty(clientId);
            //     ArgumentException.ThrowIfNullOrEmpty(clientSecrect);
            //     
            //     options.ClientId = clientId;
            //     options.ClientSecret = clientSecrect;
            //     options.CallbackPath = "/signin-google";
            // })
            .AddJwtBearer(options =>
            {
                var jwtOptions = builder.Configuration.GetSection("Jwt");
                var validIssuer = jwtOptions[nameof(JwtOptions.Issuer)];
                var validAudience = jwtOptions[nameof(JwtOptions.Audience)];
                var issuerSigningKey = jwtOptions[nameof(JwtOptions.IssuerSigningKey)];

                ArgumentException.ThrowIfNullOrEmpty(issuerSigningKey);
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ClockSkew = TokenValidationParameters.DefaultClockSkew,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = validIssuer,
                    ValidAudience = validAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey))
                };
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Family.Api", Version = "v1"});
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = HeaderNames.Authorization,
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[]{}
                }
            });
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
                var isProduction = !builder.Environment.IsDevelopment();

                options.SignIn.RequireConfirmedAccount = true;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = isProduction;
                options.Password.RequiredLength = isProduction ? 8 : 6;
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
            
        app.UseHttpsRedirection();

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