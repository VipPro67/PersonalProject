using System.Security.Cryptography.X509Certificates;
using ApiWebApp.Middlewares;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
Env.Load();
var builder = WebApplication.CreateBuilder(args);
var certPath = Environment.GetEnvironmentVariable("CertPath");
var certPassword = Environment.GetEnvironmentVariable("CertPassword");

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        listenOptions.UseHttps(new HttpsConnectionAdapterOptions
        {
            ServerCertificate = new X509Certificate2(certPath, certPassword),
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13
        });
    });
});
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();
builder.Services.AddOcelot(builder.Configuration);
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
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("Open");
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
