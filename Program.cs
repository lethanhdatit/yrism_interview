using EmployeeProfileManagement;
using EmployeeProfileManagement.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("DynamicCorsPolicy", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Base policy setup
    });
});

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Configure options to ignore null properties
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddDbContext<EmployeeContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddHttpClient<CdnUploadService>();

// Configure CdnSettings from appsettings.json
builder.Services.Configure<CdnSettings>(builder.Configuration.GetSection("CdnSettings"));

// Register CdnSettings for dependency injection
builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<CdnSettings>>().Value);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<EmployeeValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Use forwarded headers middleware
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<EmployeeContext>();
    DbInitializer.Initialize(context);
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/health");
app.MapGet("/", (HttpContext context) =>
{
    context.Response.Redirect("/health");
    return Task.CompletedTask;
});

app.UseHttpsRedirection();
// Use CORS policy
app.UseCors("DynamicCorsPolicy");

// Custom middleware to dynamically set the Access-Control-Allow-Origin header
app.Use(async (context, next) =>
{
    var origin = context.Request.Headers["Origin"].ToString();
    if (!string.IsNullOrEmpty(origin))
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", new[] { origin });
    }
    context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");

    // Handle preflight request
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 204;
        return;
    }

    await next();
});
app.MapControllers();

app.Run();
