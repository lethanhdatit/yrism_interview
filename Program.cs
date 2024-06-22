using AutoMapper;
using EmployeeProfileManagement;
using EmployeeProfileManagement.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.CreateMap<EmployeeDTO, Employee>();
    mc.CreateMap<PositionDTO, Position>();
    mc.CreateMap<ToolLanguageDTO, ToolLanguage>()
        .ForMember(dest => dest.Images, opt => opt.Ignore()); // Ignore Images mapping as it's handled separately
    mc.CreateMap<Employee, EmployeeDTO>();
    mc.CreateMap<Position, PositionDTO>();
    mc.CreateMap<ToolLanguage, ToolLanguageDTO>();
    mc.CreateMap<ImageDTO, Image>()
        .ForMember(dest => dest.Data, opt => opt.Ignore()); // Ignore data mapping as it's handled separately
    mc.CreateMap<Image, ImageDTO>();

    // New mappings
    mc.CreateMap<PositionResource, PositionResourceDTO>();
    mc.CreateMap<ToolLanguageResource, ToolLanguageResourceDTO>();
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<EmployeeContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddValidatorsFromAssemblyContaining<EmployeeValidator>();
builder.Services.AddFluentValidationAutoValidation();

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

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
