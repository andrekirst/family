using Api.Database;
using Api.Database.Core;
using Api.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;

namespace Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssemblyContaining<Program>();
                configuration.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>), ServiceLifetime.Scoped);
            });

            builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
            builder.Services.AddLocalization();
            builder.Services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
            builder.Services.AddDbContext<ApplicationDbContext>(optionsBuilder =>
            {
                var isNotProduction = !builder.Environment.IsProduction();

                optionsBuilder
                    .UseSqlServer(builder.Configuration.GetConnectionString("Default"))
                    .EnableDetailedErrors(isNotProduction)
                    .EnableSensitiveDataLogging(isNotProduction)
                    .EnableServiceProviderCaching();
            });
            builder.Services.AddUnitOfWork();
            builder.Services
                .AddGraphQLServer()
                .RegisterDbContext<ApplicationDbContext>()
                .AddQueryType<FamilyMemberQueryType>()
                .AddProjections()
                .AddFiltering()
                .AddSorting();
            builder.Services.AddAutoMapper(expression => expression.AddMaps(typeof(Program).Assembly));
            builder.Services.AddTransient<ExceptionHandlingMiddleware>();

            builder.Services.AddSingleton<ISystemClock, SystemClock>();

            var app = builder.Build();

            app.UseRequestLocalization(options =>
            {
                options.ApplyCurrentCultureToResponseHeaders = true;
                // TODO
                options.DefaultRequestCulture = new RequestCulture("de-DE");
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.MapGraphQL();
            app.MapBananaCakePop();

            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Database.MigrateAsync();

            await app.RunAsync();
        }
    }
}