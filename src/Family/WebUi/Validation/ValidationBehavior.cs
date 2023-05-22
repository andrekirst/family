using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace WebUi.Validation;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly List<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators.ToList();
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Count == 0)
        {
            return await next();
        }
        
        var errors = new List<ValidationFailure>();
        
        foreach (var validator in _validators)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                errors.AddRange(validationResult.Errors);   
            }
        }

        if (errors.Count != 0)
        {
            throw new ValidationException($"Validation failures from {_validators.Count} validators of type {typeof(TRequest)}", errors);
        }

        return await next();
    }
}
