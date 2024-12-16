using ApiWebApp.Middlewares;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
Env.Load();
var builder = WebApplication.CreateBuilder(args);
builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();
builder.Services.AddOcelot(builder.Configuration).AddCacheManager(x => x.WithDictionaryHandle());
// Add CORS
builder.Services.AddControllers();
var _JWTKeyValidIssuer = Environment.GetEnvironmentVariable("JWTKeyValidIssuer");
var _JWTKeyValidAudience = Environment.GetEnvironmentVariable("JWTKeyValidAudience");
var authSigningKey = Environment.GetEnvironmentVariable("JWTKeySecret");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = _JWTKeyValidIssuer,
        ValidateAudience = true,
        ValidAudience = _JWTKeyValidAudience,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(0),
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(authSigningKey))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Open", builder =>
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddSwaggerForOcelot(builder.Configuration);
var app = builder.Build();

app.UseRouting(); 
app.UseCustomMiddleware();
app.UseGlobalExceptionHandling();
app.UseAuthentication();
app.UseAuthorization();
app.UseAuthHeaderMiddleware();
app.MapControllers();
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

app.UseOcelot().Wait();
app.Run();
