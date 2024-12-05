using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;
using AuthApi.Helpers;
using AuthApi.Resources;
using Microsoft.Extensions.Localization;

namespace AuthApi.Filters;

public class ValidateModelStateFilter : IActionFilter
{
    private readonly LocalizationHelper _localizationHelper;

    public ValidateModelStateFilter(IStringLocalizer<Resource> localization)
    {
        _localizationHelper = new LocalizationHelper(localization);
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

        var errorMessage = _localizationHelper.GetLocalizedMessage(ResourceKey.Validation, ResourceKey.Failed);
        context.Result = new BadRequestObjectResult(new ErrorResponse(400, errorMessage, errors));
    }
}


