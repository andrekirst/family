using System.Net.Mime;
using FluentValidation;

namespace Api.Infrastructure;

internal sealed class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            logger.LogError(e, e.Message);
            await HandleExceptionAsync(context, e);
        }
    }
    private static async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var response = new
        {
            title = GetTitle(exception),
            status = statusCode,
            detail = exception.Message,
            errors = GetErrors(exception)
        };
        httpContext.Response.ContentType = MediaTypeNames.Application.Json;
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(response);
    }

    private static int GetStatusCode(Exception exception) =>
        exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

    private static string GetTitle(Exception exception) =>
        exception switch
        {
            _ => "Server Error"
        };

    private static IReadOnlyDictionary<string, string[]>? GetErrors(Exception exception)
    {
        if (exception is ValidationException validationException)
        {
            return validationException.Errors
                .GroupBy(
                    x => x.PropertyName,
                    x => x.ErrorMessage,
                    (propertyName, errorMessages) => new
                    {
                        Key = propertyName,
                        Values = errorMessages.Distinct().ToArray()
                    })
                .ToDictionary(x => x.Key, x => x.Values);
        }
        
        return null;
    }
}