namespace Family.Infrastructure.CQRS.Behaviors;

/// <summary>
/// Pipeline behavior for logging requests and responses with performance metrics
/// </summary>
/// <typeparam name="TRequest">The type of request</typeparam>
/// <typeparam name="TResponse">The type of response</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Handling {RequestName}", requestName);
        
        try
        {
            var response = await next();
            
            stopwatch.Stop();
            
            _logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex, "Error handling {RequestName} after {ElapsedMilliseconds}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
            
            throw;
        }
    }
}