using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using AuthApi.Helpers;
using AuthApi.Resources;
using Microsoft.Extensions.Localization;

namespace AuthApi.Filters;

public class ValidateModelStateFilter : IActionFilter
{
    private readonly IStringLocalizer<Resource> _localization;

    public ValidateModelStateFilter(IStringLocalizer<Resource> localization)
    {
        _localization = localization;
    }
    public void OnActionExecuted(ActionExecutedContext context)
    {

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
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            );
    
        Log.Error("Validation failed for request: {0}", context.HttpContext.Request.Path);

        var errorMessage = _localization[ResourceKey.ValidationFailed];
        context.Result = new BadRequestObjectResult(new ErrorResponse(400, errorMessage, errors));

    }
}


