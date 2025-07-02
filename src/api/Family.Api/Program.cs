using Family.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Database=family;Username=family;Password=family";

builder.Services.AddDbContext<FamilyDbContext>(options =>
    options.UseNpgsql(connectionString));

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
                context.Response.Headers.Add("Token-Expired", "true");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("FamilyUser", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("family_roles", "family-user"));
    
    options.AddPolicy("FamilyAdmin", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("family_roles", "family-admin"));
});

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

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.Run();

public partial class Program { }
