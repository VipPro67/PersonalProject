using Newtonsoft.Json;
using CourseApi.Helpers;
using Microsoft.AspNetCore.WebUtilities;
namespace CourseApi.Middlewares;
public class CustomMiddleware
{
    private readonly RequestDelegate _next;

    public CustomMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);
        var statusCode = context.Response.StatusCode;
        if (statusCode >= 400 && statusCode < 500)
        {
            context.Response.ContentType = "application/json";
            var response = new ErrorResponse(statusCode, ReasonPhrases.GetReasonPhrase(statusCode), null);
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}
public static class CustomMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomMiddleware>();
    }
}
