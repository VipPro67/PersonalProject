using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using StudentApi.Helpers;

namespace StudentApi.Filters;

public class ValidateModelStateFilter : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // let this :)
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }
      var errors = context.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .ToDictionary(
                kvp => char.ToLower(kvp.Key[0]) + kvp.Key.Substring(1),
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        Log.Error("Validation failed for request: {0}", context.HttpContext.Request.Path);
        context.Result = new BadRequestObjectResult(new ErrorResponse(400, "Validation failed", errors));
    }
}

