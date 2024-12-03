using System.Text.Json.Serialization;
using CourseApi;
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
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(Environment.GetEnvironmentVariable("CONNECTION_STRING")));
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssemblyContaining<CreateCourseDto>()
    .AddValidatorsFromAssemblyContaining<UpdateCourseDto>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers(option =>
    {
        option.Filters.Add(typeof(ValidateModelStateFilter));
    })
    .ConfigureApiBehaviorOptions(options => options.SuppressModelStateInvalidFilter = true)
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddAutoMapper(typeof(CourseMappingProfile));
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
    options.AddPolicy("AllowCors",
        builder => builder
            .WithOrigins(Environment.GetEnvironmentVariable("CORS_ORIGINS").Split(','))
            .AllowAnyHeader()
            .WithMethods("GET", "POST", "PUT", "DELETE"));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowCors");
app.UseGlobalExceptionHandling();
app.UseSerilogRequestLogging();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
app.Run();

