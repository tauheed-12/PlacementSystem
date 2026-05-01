using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PlacementDriveService.Data;
using PlacementDriveService.Middleware;
using PlacementDriveService.Repositries;
using PlacementDriveService.Repositries.Interfaces;
using Common.Contracts.Infrastructure;
using PlacementDriveService.Services;
using PlacementDriveService.Services.Interfaces;
using FluentValidation;
using PlacementDriveService.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddDbContext<PlacementDriveDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssemblyContaining<DriveCreateRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<DriveUpdateRequestValidator>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<RequestContextAccessor>();

builder.Services.AddSingleton<IKafkaClient, KafkaClient>();
builder.Services.AddScoped<IPlacementDriveService, PlacementDriveService.Services.PlacementDriveService>();
builder.Services.AddScoped<IPlacementDriveRepository, PlacementDriveRepository>();

builder.Services.AddHostedService<OutboxProcessor>();

builder.Services.AddAuthorization();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<PlacementDriveDbContext>("database");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Drive Service API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<LocalGatewaySimulationMiddleware>();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseGlobalExceptionMiddleware();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();