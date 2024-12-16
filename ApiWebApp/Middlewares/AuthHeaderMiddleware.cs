using System.Security.Claims;
public class AuthHeaderMiddleware
{
    private readonly RequestDelegate _next;

    public AuthHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity.IsAuthenticated)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = context.User.FindFirstValue(ClaimTypes.Name);
            var email = context.User.FindFirstValue(ClaimTypes.Email);
            Console.WriteLine($"User authenticated: UserId: {userId}, UserName: {userName}, Email: {email}");
            context.Request.Headers.Add("X-UserId", userId);
            context.Request.Headers.Add("X-UserName", userName);
            context.Request.Headers.Add("X-Email", email);
            //context.User = new ClaimsPrincipal();
            //context.Request.Headers.Remove("Authorization");
        }
        await _next(context);
    }
}

public static class AuthHeaderMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthHeaderMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthHeaderMiddleware>();
    }
}