using Family.Infrastructure.CQRS.Abstractions;

namespace Family.Infrastructure.CQRS.Behaviors;

/// <summary>
/// Pipeline behavior for validating commands and queries using FluentValidation
/// </summary>
/// <typeparam name="TRequest">The type of request</typeparam>
/// <typeparam name="TResponse">The type of response</typeparam>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;
        
        _logger.LogDebug("Validating {RequestName}", requestName);

        var context = new ValidationContext<TRequest>(request);
        
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count > 0)
        {
            _logger.LogWarning("Validation failed for {RequestName}: {Errors}", 
                requestName, string.Join(", ", failures.Select(f => f.ErrorMessage)));

            var errorDictionary = failures
                .GroupBy(x => x.PropertyName, x => x.ErrorMessage)
                .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());

            // If response type is CommandResult, return validation failure
            if (typeof(TResponse) == typeof(CommandResult))
            {
                return (TResponse)(object)CommandResult.ValidationFailure(errorDictionary);
            }

            // If response type is CommandResult<T>, return validation failure
            if (typeof(TResponse).IsGenericType && 
                typeof(TResponse).GetGenericTypeDefinition() == typeof(CommandResult<>))
            {
                var commandResultType = typeof(CommandResult<>).MakeGenericType(typeof(TResponse).GenericTypeArguments[0]);
                var validationFailureMethod = commandResultType.GetMethod("ValidationFailure");
                return (TResponse)validationFailureMethod!.Invoke(null, new object[] { errorDictionary })!;
            }

            // For other types, throw ValidationException
            throw new ValidationException(failures);
        }

        _logger.LogDebug("Validation succeeded for {RequestName}", requestName);
        
        return await next();
    }
}