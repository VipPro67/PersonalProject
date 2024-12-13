using Newtonsoft.Json;
using ApiWebApp.Helpers;
namespace ApiWebApp.Middlewares;

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
        if (context.Response.StatusCode == 401)
        {
            context.Response.ContentType = "application/json";
            var response = new ErrorResponse(401, "Unauthorized: Authentication is required to access this resource", null);
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
        else if (context.Response.StatusCode == 403)
        {
            context.Response.ContentType = "application/json";
            var response = new ErrorResponse(403, "Forbidden: You do not have permission to access this resource", null);
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
        else if (context.Response.StatusCode == 405)
        {
            context.Response.ContentType = "application/json";
            var response = new ErrorResponse(405, "Not allowed: This method not allowed", null);
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
        else if (context.Response.StatusCode == 415)
        {
            context.Response.ContentType = "application/json";
            var response = new ErrorResponse(415, "Unsupported Media Type", null);
            await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
        else if (context.Response.StatusCode == 502)
        {
            context.Response.ContentType = "application/json";
            var response = new ErrorResponse(502, "Bad Gateway: The server is temporarily unable to handle the request", null);
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

