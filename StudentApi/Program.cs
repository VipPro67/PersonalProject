using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Serilog;
using StudentApi.Data;
using StudentApi.DTOs;
using StudentApi.Filters;
using StudentApi.Helpers;
using StudentApi.Mappings;
using StudentApi.Middlewares;
using StudentApi.Repositories;
using StudentApi.Services;

Env.Load();
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Student API", Version = "v1" });
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
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssemblyContaining<CreateStudentDto>()
    .AddValidatorsFromAssemblyContaining<UpdateStudentDto>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers(option =>
    {
        option.Filters.Add(typeof(ValidateModelStateFilter));
    })
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Converters.Add(new DateOnlyJsonConverter());
    })
    .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true);
builder.Services.AddAutoMapper(typeof(StudentMappingProfile));
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
//app.UseAuthentication();
//app.UseAuthorization();
app.Run();
