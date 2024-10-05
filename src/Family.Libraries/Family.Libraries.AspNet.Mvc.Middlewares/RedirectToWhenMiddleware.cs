namespace Family.Libraries.AspNet.Mvc.Middlewares;

public class RedirectToWhenMiddleware(RequestDelegate next, string sourcePath, string targetPath)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourcePath);
        ArgumentException.ThrowIfNullOrEmpty(targetPath);
        
        if (httpContext.Request.Path == sourcePath)
        {
            httpContext.Response.Redirect(targetPath);
            return;
        }

        await next(httpContext);
    }
}

public static class RedirectToWhenMiddlewareExtensions
{
    public static void UseRedirectToWhen(this IApplicationBuilder builder, string sourcePath, string targetPath)
    {
        builder.UseMiddleware<RedirectToWhenMiddleware>(sourcePath, targetPath);
    }
}