using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StudentService.Data;
using StudentService.Repositories;
using StudentService.Repositories.Interfaces;
using StudentService.Middleware;
using StudentService.Services.Interfaces;
using StudentService.Services;
using StudentService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// Configuration
// ----------------------------------------------------
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// ----------------------------------------------------
// Database Configuration
// ----------------------------------------------------
builder.Services.AddDbContext<StudentDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("StudentDb"),
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
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService.Services.StudentService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<RequestContextAccessor>();

// ----------------------------------------------------
// Health Checks
// ----------------------------------------------------
builder.Services.AddHealthChecks()
    .AddDbContextCheck<StudentDbContext>("database");

// ----------------------------------------------------
// Swagger
// ----------------------------------------------------
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "StudentService API",
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

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "GatewayHeaders",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
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

app.UseHttpsRedirection();
app.UseGlobalExceptionMiddleware();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();