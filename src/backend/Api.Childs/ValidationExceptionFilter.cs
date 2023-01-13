using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Childs;

public class ValidationExceptionFilter : IActionFilter, IOrderedFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is not ValidationException validationException)
        {
            return;
        }

        context.Result = new BadRequestObjectResult(validationException.Errors);

        context.ExceptionHandled = true;
    }

    public int Order => int.MaxValue - 10;
}