using AuthApi.Data;
using AuthApi.Repositories;
using AuthApi.Services;
using AuthApi.Resources;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using AuthApi.Validators;
using FluentValidation.AspNetCore;
using AuthApi.Filters;
using Serilog;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using AuthApi.Middlewares;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Security.Cryptography.X509Certificates;

Env.Load();

var builder = WebApplication.CreateBuilder(args);
var certPath = Environment.GetEnvironmentVariable("CertPath");
var certPassword = Environment.GetEnvironmentVariable("CertPassword");
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5005, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        listenOptions.UseHttps(new HttpsConnectionAdapterOptions
        {
            ServerCertificate = new X509Certificate2(certPath, certPassword),
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13
        });
    });
});
// Serilog configuration
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
// Add services to the container and configure them
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
}); builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStringW")));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddSingleton<IStringLocalizer, StringLocalizer<Resource>>();

builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssemblyContaining<RegisterDtoValidator>()
    .AddValidatorsFromAssemblyContaining<LoginDtoValidator>();
builder.Services.AddControllers(options => options.Filters.Add(typeof(ValidateModelStateFilter)))
    .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true)
    .AddDataAnnotationsLocalization();

// Localization configuration
builder.Services.AddLocalization(
    options => options.ResourcesPath = "");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    List<CultureInfo> supportedCultures = new List<CultureInfo>{
        new CultureInfo("en-US"),
        new CultureInfo("vi-VN")
    };
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new AcceptLanguageHeaderRequestCultureProvider());
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = Environment.GetEnvironmentVariable("JWTKeyValidIssuer"),
        ValidateAudience = true,
        ValidAudience = Environment.GetEnvironmentVariable("JWTKeyValidAudience"),
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(0), // If clock skew > 0. This will make token exprire can continues to be valid. Default 300 secs
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWTKeySecret")))
    };
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAPIGateWay",
        builder => builder
            .WithOrigins(Environment.GetEnvironmentVariable("APIGateWay").Split(','))
            .AllowAnyHeader()
            .WithMethods("POST"));
});


var app = builder.Build();
var opts = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
if (opts != null)
{
    app.UseRequestLocalization(opts.Value);
}
else
{
    throw new InvalidOperationException("RequestLocalizationOptions is not configured properly.");
}

app.UseSerilogRequestLogging();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseCors("AllowAPIGateWay");
app.UseGlobalExceptionHandling();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
