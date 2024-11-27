using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using AuthApi.Services;

namespace AuthApi.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class JwtAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();

            var token = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "No token provided" });
                return;
            }
            var claims = authService.VerifyToken(token);
            if (claims == null)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Invalid token" });
                return;
            }
            
            context.HttpContext.User = authService.GetClaimsPrincipal(token);

            await next();
        }
    }
}