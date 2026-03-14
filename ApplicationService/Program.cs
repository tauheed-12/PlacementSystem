using ApplicationService.Data;
using ApplicationService.HttpClients;
using ApplicationService.HttpClients.Interfaces;
using ApplicationService.Repositories;
using ApplicationService.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// -------------------------------------------------------
// Configuration
// -------------------------------------------------------
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();


// -------------------------------------------------------
// Database Configuration
// -------------------------------------------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ApplicationDb"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        })
);


// -------------------------------------------------------
// External HTTP Clients
// -------------------------------------------------------
var placementDriveBaseUrl =
    builder.Configuration["Services:PlacementDriveService:BaseUrl"]
    ?? throw new InvalidOperationException(
        "Configuration error: 'Services:PlacementDriveService:BaseUrl' is missing."
    );

builder.Services.AddHttpClient<IPlacementDriveServiceClient>(client =>
{
    client.BaseAddress = new Uri(placementDriveBaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(10);
});


// -------------------------------------------------------
// JWT Authentication
// -------------------------------------------------------
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("Configuration error: 'Jwt:Key' is missing.");
}

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
if (keyBytes.Length < 16)
{
    throw new InvalidOperationException("Configuration error: 'Jwt:Key' is too short.");
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


// -------------------------------------------------------
// Application Services & Repositories
// -------------------------------------------------------
builder.Services.AddScoped<ApplicationDbContext>();
builder.Services.AddScoped<ApplicationService.Data.Interfaces.IUnitOfWork, ApplicationService.Data.UnitOfWork>();
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<ApplicationService.Services.Interfaces.IApplicationService,
                           ApplicationService.Services.ApplicationService>();
builder.Services.AddScoped<IPlacementDriveServiceClient, PlacementDriveServiceClient>();


// -------------------------------------------------------
// Controllers
// -------------------------------------------------------
builder.Services.AddControllers();


// -------------------------------------------------------
// Health Checks
// -------------------------------------------------------
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");


// -------------------------------------------------------
// Swagger + JWT Authorization
// -------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ApplicationService API",
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


// -------------------------------------------------------
// Build Application
// -------------------------------------------------------
var app = builder.Build();


// -------------------------------------------------------
// Middleware Pipeline
// -------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ApplicationService.Middleware.GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
