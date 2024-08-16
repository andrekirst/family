using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Api.Infrastructure;

public class ValidationPipelineBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var validationFailures = new List<ValidationFailure>();

        foreach (var validator in validators)
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                validationFailures.AddRange(validationResult.Errors);
            }
        }

        if (validationFailures.Count != 0)
        {
            throw new ValidationException(validationFailures);
        }

        return await next();
    }
}
