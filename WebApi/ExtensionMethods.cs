using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace WebApi
{
    public static class ExtensionMethods
    {
        public static string GetModelStateError(this ModelStateDictionary modelState)
        {
            var message = string.Join(" | ", modelState.Values
             .SelectMany(v => v.Errors)
             .Select(e => e.ErrorMessage));
            string errorMessage = null;
            foreach (var item in message)
            {
                errorMessage += item;
            }
            return errorMessage;
        }

    }
}
