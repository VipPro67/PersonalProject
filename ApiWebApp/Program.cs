using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddOcelot(builder.Configuration);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Open", builder =>
        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});
var app = builder.Build();


app.UseHttpsRedirection();

await app.UseOcelot();
app.Run();
