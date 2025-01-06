using System.Security.Cryptography.X509Certificates;
using CourseApi.Data;
using CourseApi.DTOs;
using CourseApi.Filters;
using CourseApi.Helpers;
using CourseApi.Mappings;
using CourseApi.Middlewares;
using CourseApi.Repositories;
using CourseApi.Services;
using DotNetEnv;
using FluentValidation;
using Grpc.Net.Client;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Serilog;
using Microsoft.Extensions.Caching.Hybrid;

Env.Load();
var builder = WebApplication.CreateBuilder(args);
var certPath = Environment.GetEnvironmentVariable("CertPath");
var certPassword = Environment.GetEnvironmentVariable("CertPassword");
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
        listenOptions.UseHttps(new HttpsConnectionAdapterOptions
        {
            ServerCertificate = new X509Certificate2(certPath, certPassword),
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13
        });
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Course API", Version = "v1" });
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
});
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Environment.GetEnvironmentVariable("ConnectionStringW")));
builder.Services.AddAutoMapper(typeof(CourseMappingProfile));
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddSingleton(provider =>
{
    var studentApiUrl = Environment.GetEnvironmentVariable("StudentApiUrl");
    return GrpcChannel.ForAddress(studentApiUrl, new GrpcChannelOptions
    {
        HttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        }
    });
});

builder.Services.AddTransient<CourseApi.Protos.StudentService.StudentServiceClient>(provider =>
{
    var channel = provider.GetRequiredService<GrpcChannel>();
    return new CourseApi.Protos.StudentService.StudentServiceClient(channel);
});

builder.Services.AddFluentValidationAutoValidation()

    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssemblyContaining<CreateCourseDto>()
    .AddValidatorsFromAssemblyContaining<UpdateCourseDto>()
    .AddValidatorsFromAssemblyContaining<CreateEnrollmentDto>();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers(option =>
    {
        option.Filters.Add(typeof(ValidateModelStateFilter));
    })
    .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true)
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new DateOnlyJsonConverter());
    });
builder.Services.AddGrpc();


builder.Services.AddHybridCache();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = Environment.GetEnvironmentVariable("RedisConnectionString");
});
/*
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
*/
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCors",
        builder => builder
            .WithOrigins(Environment.GetEnvironmentVariable("CorsOrigins").Split(','))
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("X-UserId", "X-UserName", "X-Email"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseRouting();
app.UseCors("AllowCors");
app.UseUserInfoLogging();
app.UseGlobalExceptionHandling();
app.UseSerilogRequestLogging();
// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapGrpcService<EnrollmentGrpcService>();
//app.UseAuthentication();
//app.UseAuthorization();
app.Run();

