using Serilog.Context;

namespace CourseApi.Middlewares;
public class UserInfoLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public UserInfoLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        //var userId = context.Request.Headers["X-UserId"].FirstOrDefault();
        var userName = context.Request.Headers["X-UserName"].FirstOrDefault();
       // var email = context.Request.Headers["X-Email"].FirstOrDefault();

        //Log.Information("User Info - UserId: {UserId}, UserName: {UserName}, Email: {Email}", userId, userName, email);
       // using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("UserName", userName))
        //using (LogContext.PushProperty("Email", email))
        {
            await _next(context);
        }
    }

}

public static class UserInfoLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseUserInfoLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserInfoLoggingMiddleware>();
    }
}
