using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using StudentService.Data;
using StudentService.Repositories;
using StudentService.Repositories.Interfaces;
using StudentService.Middleware;
using StudentService.Services.Interfaces;
using StudentService.Services;
using Common.Contracts.Infrastructure;
using StudentService.Validators;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// Configuration
// ----------------------------------------------------
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// -----------------------------------------------------
// Database Configuration
// -----------------------------------------------------
builder.Services.AddDbContext<StudentDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

// ----------------------------------------------------
// Controllers
// ----------------------------------------------------
builder.Services.AddControllers();

// Register all validators automatically
builder.Services.AddValidatorsFromAssemblyContaining<CreateStudentProfileValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateStudentProfileValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<BulkUserIdsValidator>();

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
        Title = "Student Service API",
        Version = "v1"
    });

    // JWT Bearer Auth definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: eyJhbGci..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
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
    app.UseMiddleware<LocalGatewaySimulationMiddleware>();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseGlobalExceptionMiddleware();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();