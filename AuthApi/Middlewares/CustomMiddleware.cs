using Newtonsoft.Json;
using AuthApi.Helpers;
namespace AuthApi.Middlewares;
public class CustomMiddleware
{
    private readonly RequestDelegate _next;

    public CustomMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";

            switch (context.Response.StatusCode)
            {
                case 401:
                    var unauthorizedResponse = new ErrorResponse(401, "Unauthorized: Authentication is required to access this resource", null);
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(unauthorizedResponse));
                    break;
                case 403:
                    var forbiddenResponse = new ErrorResponse(403, "Forbidden: You do not have permission to access this resource", null);
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(forbiddenResponse));
                    break;
                case 405:
                    var notAllowedResponse = new ErrorResponse(405, "Not allowed: This method not allowed", null);
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(notAllowedResponse));
                    break;
                case 415:
                    var unsupportedMediaResponse = new ErrorResponse(415, "Unsupported Media Type", null);
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(unsupportedMediaResponse));
                    break;
                default:
                    var generalErrorResponse = new ErrorResponse(context.Response.StatusCode, "An error occurred", ex.Message);
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(generalErrorResponse));
                    break;
            }
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

