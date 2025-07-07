using System.Globalization;
using Family.Api.Authorization;
using Family.Api.Data;
using Family.Api.Data.Interceptors;
using Family.Api.Extensions;
using Family.Api.GraphQL.Mutations;
using Family.Api.GraphQL.Queries;
using Family.Api.GraphQL.Types;
using Family.Api.Services;
using Family.Infrastructure.Caching.Extensions;
using Family.Infrastructure.CQRS.Extensions;
using Family.Infrastructure.EventSourcing.Extensions;
using Family.Infrastructure.Messaging.Extensions;
using Family.Infrastructure.Resilience.Extensions;
using HotChocolate.Authorization;
using HotChocolate.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FamilyAppPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200",
                "http://localhost:3000",
                "https://localhost:3000"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=family;Username=family;Password=family";

builder.Services.AddDbContext<FamilyDbContext>(options =>
    options.UseNpgsql(connectionString)
           .AddInterceptors(new AuditableEntityInterceptor()));

// Register caching services
builder.Services.AddFamilyCaching(builder.Configuration);

// Register CQRS services
builder.Services.AddCQRS(typeof(Program).Assembly);

// Register resilience services
builder.Services.AddResilience(builder.Configuration);

// Register Event Store services
builder.Services.AddEventSourcing(builder.Configuration);

// Register Kafka messaging services
builder.Services.AddKafkaMessaging(builder.Configuration);

// Register Family services
builder.Services.AddScoped<Family.Api.Features.Families.IFamilyRepository, Family.Api.Features.Families.FamilyRepository>();
builder.Services.AddScoped<Family.Api.Services.IDomainEventPublisher, Family.Api.Services.DomainEventPublisher>();
builder.Services.AddScoped<Family.Api.Services.IFamilyRoleAssignmentService, Family.Api.Services.FamilyRoleAssignmentService>();

// Register User services
builder.Services.AddScoped<Family.Api.Features.Users.Services.IFirstTimeUserService, Family.Api.Features.Users.Services.FirstTimeUserService>();

// Register Kafka Event Handlers
builder.Services.AddHostedService<Family.Api.Features.Families.EventHandlers.KafkaEventHandlerService>();

// Register health checks
builder.Services.AddFamilyHealthChecks(builder.Configuration);

// Add health checks UI for development
builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

// Configure HealthChecksUI from configuration
builder.Services.Configure<HealthChecks.UI.Configuration.Settings>(
    builder.Configuration.GetSection("HealthChecksUI"));

// Configure Localization
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("de"),
        new CultureInfo("en")
    };
    
    options.DefaultRequestCulture = new RequestCulture("de");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
});

// Register services (temporarily using non-cached version for debugging)
builder.Services.AddHttpClient<IKeycloakService, KeycloakService>();
builder.Services.AddScoped<IKeycloakService, KeycloakService>();

// Configure Keycloak Authentication
var keycloakAuthority = builder.Configuration["Keycloak:Authority"] 
    ?? "http://localhost:8080/realms/family";
var keycloakAudience = builder.Configuration["Keycloak:Audience"] 
    ?? "family-api";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.Audience = keycloakAudience;
        options.RequireHttpsMetadata = false; // Only for development
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
        
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.Response.Headers["Token-Expired"] = "true";
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.FamilyUser, policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim(Claims.FamilyRoles, Roles.FamilyUser));
    
    options.AddPolicy(Policies.FamilyAdmin, policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim(Claims.FamilyRoles, Roles.FamilyAdmin));
});

// Configure GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<UserQueries>()
    .AddTypeExtension<UserCQRSQueries>()
    .AddTypeExtension<AuthenticationMutations>()
    .AddTypeExtension<UserMutations>()
    .AddTypeExtension<Family.Api.GraphQL.Queries.FamilyQueries>()
    .AddTypeExtension<Family.Api.GraphQL.Mutations.FamilyMutations>()
    .AddTypeExtension<Family.Api.GraphQL.Queries.FamilyRoleQueries>()
    .AddTypeExtension<Family.Api.GraphQL.Mutations.FamilyRoleMutations>()
    .AddType<UserType>()
    .AddType<LoginInputType>()
    .AddType<LoginCallbackInputType>()
    .AddType<RefreshTokenInputType>()
    .AddType<LoginInitiationPayloadType>()
    .AddType<LoginPayloadType>()
    .AddType<LogoutPayloadType>()
    .AddType<RefreshTokenPayloadType>()
    .AddType<Family.Api.GraphQL.Types.FamilyType>()
    .AddType<Family.Api.GraphQL.Types.FamilyMemberType>()
    .AddType<Family.Api.GraphQL.Types.CreateFamilyInputType>()
    .AddType<Family.Api.GraphQL.Types.CreateFamilyPayloadType>()
    .AddType<Family.Api.GraphQL.Types.FirstTimeUserInfoType>()
    .AddType<Family.Api.GraphQL.Types.FamilyMemberRoleType>()
    .AddType<Family.Api.GraphQL.Types.AssignRoleInputType>()
    .AddType<Family.Api.GraphQL.Types.AssignRoleResultType>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("FamilyAppPolicy");

// Request Localization
app.UseRequestLocalization();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// GraphQL endpoint
app.MapGraphQL();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

// Health checks UI
if (app.Environment.IsDevelopment())
{
    app.MapHealthChecksUI(options => options.UIPath = "/health-ui");
}

app.Run();

public partial class Program { }
