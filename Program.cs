using EmployeeProfileManagement;
using EmployeeProfileManagement.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

const string CorsSpecificOrigins = "CorsSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsSpecificOrigins,
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .AllowAnyMethod();
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
app.UseCors(CorsSpecificOrigins);
app.MapControllers();

app.Run();
