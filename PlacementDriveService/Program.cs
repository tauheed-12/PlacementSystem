using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PlacementDriveService.Data;
using PlacementDriveService.Services;
using PlacementDriveService.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
// JWT Authentication
// ----------------------------------------------------
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "Configuration error: 'Jwt:Key' is missing."
    );
}

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

if (keyBytes.Length < 16)
{
    throw new InvalidOperationException(
        "Configuration error: 'Jwt:Key' is too short. Provide at least 128 bits."
    );
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();


// ----------------------------------------------------
// Controllers
// ----------------------------------------------------
builder.Services.AddControllers();


// ----------------------------------------------------
// Application Services
// ----------------------------------------------------
builder.Services.AddSingleton<IKafkaClient, KafkaClient>();

builder.Services.AddScoped<IPlacementDriveService,
    PlacementDriveService.Services.PlacementDriveService>();

builder.Services.AddScoped<
    PlacementDriveService.Repositries.Interfaces.IPlacementDriveRepository,
    PlacementDriveService.Repositries.PlacementDriveRepository>();


// ----------------------------------------------------
// Health Checks
// ----------------------------------------------------
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PlacementDriveDbContext>("database");


// ----------------------------------------------------
// Swagger + JWT Authorization
// ----------------------------------------------------
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PlacementDrive API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter token as: Bearer {your token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Id = "Bearer",
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

app.UseMiddleware<PlacementDriveService.Middleware.GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
