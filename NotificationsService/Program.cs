using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NotificationsService.Clients;
using NotificationsService.Clients.Interfaces;
using NotificationsService.Consumers;
using NotificationsService.Data;
using NotificationsService.Repositories;
using NotificationsService.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();


// Controllers
builder.Services.AddControllers();


// -------------------------------------
// Swagger
// -------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Token"
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
            new string[]{}
        }
    });
});


// -----------------------------------------------------
// Database
// -----------------------------------------------------
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("NotificationDb"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            5,
            TimeSpan.FromSeconds(10),
            null
        )
    )
);


// -----------------------------------------------------
// JWT Authentication
// -----------------------------------------------------
var jwtKey = builder.Configuration["Jwt:Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey))
    };
});


// -----------------------------------------------------
// Background Services client
// -----------------------------------------------------
builder.Services.AddSingleton<IKafkaClient, KafkaClient>();
builder.Services.AddSingleton<IEmailClient, EmailClient>();

// -----------------------------------------------------
// HTTP Clients
// -----------------------------------------------------
builder.Services.AddHttpClient<IStudentServiceClient, StudentServiceClient>(client =>
{
    var baseUrl = builder.Configuration["Services:StudentService:BaseUrl"];
    if (string.IsNullOrEmpty(baseUrl))
    {
        throw new InvalidOperationException("Base URL for Student Service is not configured.");
    }
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});


// -----------------------------------------------------
// Repositories
// -----------------------------------------------------
builder.Services.AddScoped<INotificationIntentRepo, NotificationIntentRepo>();
builder.Services.AddScoped<INotificationDeliveryRepo, NotificationDeliveryRepo>();
builder.Services.AddScoped<IInAppNotificationRepo, InAppNotificationRepo>();
builder.Services.AddScoped<IUserPreferenceRepo, UserPreferenceRepo>();


// -----------------------------------------------------
// Background workers
// -----------------------------------------------------
builder.Services.AddHostedService<EventConsumer>();
builder.Services.AddHostedService<InAppDeliveryWorker>();
builder.Services.AddHostedService<EmailDeliveryWorker>();


// -----------------------------------------------------
// Health Checks
// -----------------------------------------------------
builder.Services.AddHealthChecks();


var app = builder.Build();


// -----------------------------------------------------
// Middleware pipeline
// -----------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<NotificationsService.Middleware.GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.UseAuthentication();   // IMPORTANT
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
