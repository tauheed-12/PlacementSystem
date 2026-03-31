using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PlacementDriveService.Data;
using PlacementDriveService.Middleware;
using PlacementDriveService.Repositries;
using PlacementDriveService.Repositries.Interfaces;
using PlacementDriveService.Services;
using PlacementDriveService.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// Configuration
// ----------------------------------------------------
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// ----------------------------------------------------
// Logging
// ----------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ----------------------------------------------------
// Database Configuration
// ----------------------------------------------------
builder.Services.AddDbContext<PlacementDriveDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PlacementDriveDb"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        })
);

// ----------------------------------------------------
// Controllers
// ----------------------------------------------------
builder.Services.AddControllers();

// ----------------------------------------------------
// HTTP Context Accessor
// Needed to read X-User-Id and X-User-Role headers
// forwarded by the API Gateway
// ----------------------------------------------------
builder.Services.AddHttpContextAccessor();

// ----------------------------------------------------
// Application Services & Repositories
// ----------------------------------------------------
builder.Services.AddSingleton<IKafkaClient, KafkaClient>();

builder.Services.AddScoped<IPlacementDriveService, PlacementDriveService.Services.PlacementDriveService>();
builder.Services.AddScoped<IPlacementDriveRepository, PlacementDriveRepository>();

// ----------------------------------------------------
// Health Checks
// ----------------------------------------------------
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PlacementDriveDbContext>("database");

// ----------------------------------------------------
// Swagger
// ----------------------------------------------------
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PlacementDrive API",
        Version = "v1"
    });

    // Simulates gateway header forwarding during local dev testing
    options.AddSecurityDefinition("GatewayHeaders", new OpenApiSecurityScheme
    {
        Name = "X-User-Id",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Paste a user GUID to simulate gateway forwarding"
    });

    options.AddSecurityDefinition("X-User-Role", new OpenApiSecurityScheme
    {
        Name = "X-User-Role",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "User Role (e.g. admin, student)"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "X-User-Id"
                }
            },
            new string[] {}
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "X-User-Role"
                }
            },
            new string[] {}
        }
    });
});

// ----------------------------------------------------
// Build Application
// ----------------------------------------------------
var app = builder.Build();

// ----------------------------------------------------
// Middleware Pipeline
// ----------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<PlacementDriveService.Middleware.GlobalExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseGlobalExceptionMiddleware();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();