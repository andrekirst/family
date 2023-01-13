using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Serilog;
using Serilog.Exceptions;
using System.Net.Mime;
using System.Text.Json.Serialization;
using Api.Childs.Database;
using Api.Childs.Database.Repositories;
using Api.Childs.Infrastructure;
using FluentValidation.Results;
using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Api.Childs;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.Console()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .CreateLogger();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
        // TODO
        //.EnableTokenAcquisitionToCallDownstreamApi()
        //    .AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamApi"))
        //    .AddInMemoryTokenCaches();

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default") ?? throw new ConnectionStringNotFoundException());
        });

        builder.Services.AddRepositories();
        builder.Services.AddUnitOfWork();

        builder.Services
            .AddControllers(options =>
            {
                //var scopes = builder.Configuration["AzureAd:Scopes"]?.Split(' ').ToArray();
                //if (scopes is not null && scopes.Any())
                //{
                //    //options.Filters.Add(new RequiredScopeAttribute(scopes));
                //    //options.Filters.Add(new RequiredScopeAttribute());
                //}

                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
                options.Filters.Add(new ProducesResponseTypeAttribute(typeof(List<ValidationFailure>), StatusCodes.Status400BadRequest));
                options.Filters.Add(new ProducesResponseTypeAttribute(typeof(ProblemDetails), StatusCodes.Status500InternalServerError));
                options.Filters.Add(new ConsumesAttribute(MediaTypeNames.Application.Json));
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        var programType = typeof(Program);
        var domainAssembly = programType.Assembly;
        builder.Services.AddMediatR(domainAssembly);
        builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        builder.Services.AddValidatorsFromAssembly(domainAssembly);
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var mappingConfig = new MapperConfiguration(config =>
        {
            config.AddMaps(programType);
        });
        var mapper = mappingConfig.CreateMapper();
        builder.Services.AddSingleton(mapper);

        builder.Services.AddRouting(options => options.LowercaseUrls = true);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}