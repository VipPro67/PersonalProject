using Newtonsoft.Json;
using ApiWebApp.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
namespace ApiWebApp.Middlewares
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();
            var path = context.Request.Path.Value.ToLower();

            if (path != "/api/auth/register" && path != "/api/auth/login")
            {
                if (string.IsNullOrEmpty(token))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorResponse(401, "Unauthorized", "Token is missing")));
                    return;
                }

                var isValid = VerifyToken(token);
                if (!isValid)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorResponse(401, "Unauthorized", "Invalid token")));
                    return;
                }

                context.User = GetClaimsPrincipal(token);
            }
            var UserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var UserName = context.User.FindFirstValue(ClaimTypes.Name);
            var Email = context.User.FindFirstValue(ClaimTypes.Email);
            Console.WriteLine($"User {UserId}! {UserName} {Email} accessed the endpoint.");


            await _next(context);
        }
        public bool VerifyToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWTKeySecret"));

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = Environment.GetEnvironmentVariable("JWTKeyValidIssuer"),
                    ValidateAudience = true,
                    ValidAudience = Environment.GetEnvironmentVariable("JWTKeyValidAudience"),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public ClaimsPrincipal GetClaimsPrincipal(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWTKeySecret"));
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = Environment.GetEnvironmentVariable("JWTKeyValidIssuer"),
                    ValidateAudience = true,
                    ValidAudience = Environment.GetEnvironmentVariable("JWTKeyValidAudience"),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token validation failed: {ex.Message}");
                return null;
            }
        }
    }
    public static class AuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthMiddleware>();
        }
    }
}
