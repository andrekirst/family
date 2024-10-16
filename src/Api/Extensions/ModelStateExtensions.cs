using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.Extensions;

public static class ModelStateExtensions
{
    public static void AddIdentityModelErrors(this ModelStateDictionary modelState, IEnumerable<IdentityError>? errors)
    {
        if (errors != null)
        {
            modelState.AddModelErrors(errors, key => key.Code, message => message.Description);   
        }
    }

    public static void AddModelErrors<T>(this ModelStateDictionary modelState, IEnumerable<T> list, Func<T, string> key, Func<T, string> message)
    {
        foreach (var item in list)
        {
            modelState.AddModelError(key(item), message(item));
        }
    }
}