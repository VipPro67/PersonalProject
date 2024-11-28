using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace AuthApi.Filters
{
    public class ValidateModelStateFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // let this :)
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            
            Log.Warning($"Current Accept-Language header: {context.HttpContext.Request.Headers["Accept-Language"]}");
            if (context.ModelState.IsValid)
            {
                return;
            }
            var errors = context.ModelState
                    .Where(e => e.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
            
            Log.Error("Validation failed for request: {0}", context.HttpContext.Request.Path);
            context.Result = new BadRequestObjectResult(new ErrorResponse(400, "Validation failed", errors));
        }
    }
}
