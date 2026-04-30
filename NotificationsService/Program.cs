using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NotificationsService.Clients;
using NotificationsService.Clients.Interfaces;
using NotificationsService.Consumers;
using Common.Contracts.Configuration;
using Common.Contracts.Infrastructure;
using NotificationsService.Data;
using NotificationsService.Middleware;
using NotificationsService.Repositories;
using NotificationsService.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------
// Configuration
// -------------------------------------------------------
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// -------------------------------------------------------
// Controllers
// -------------------------------------------------------
builder.Services.AddControllers();

// -------------------------------------------------------
// HTTP Context Accessor
// Needed to read X-User-Id and X-User-Role headers
// forwarded by the API Gateway
// -------------------------------------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<RequestContextAccessor>();

// -------------------------------------------------------
// Swagger
// -------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NotificationsService API",
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

// -------------------------------------------------------
// Database
// -------------------------------------------------------
builder.Services.AddDbContext<NotificationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));


// -------------------------------------------------------
// Background Service Clients
// -------------------------------------------------------
builder.Services.AddSingleton<IKafkaClient, KafkaClient>();
builder.Services.AddSingleton<IEmailClient, EmailClient>();

// -------------------------------------------------------
// HTTP Clients
// -------------------------------------------------------
builder.Services.AddOptions<ServiceEndpointOptions>("StudentService")
    .Bind(builder.Configuration.GetSection("Services:StudentService"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<IStudentServiceClient, StudentServiceClient>((sp, client) =>
{
    var studentServiceOptions = sp.GetRequiredService<IOptionsMonitor<ServiceEndpointOptions>>().Get("StudentService");
    var baseUrl = studentServiceOptions.BaseUrl;

    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddPolicyHandler(GetRetryPolicy());

// -------------------------------------------------------
// Repositories
// -------------------------------------------------------
builder.Services.AddScoped<INotificationIntentRepo, NotificationIntentRepo>();
builder.Services.AddScoped<INotificationDeliveryRepo, NotificationDeliveryRepo>();
builder.Services.AddScoped<IInAppNotificationRepo, InAppNotificationRepo>();
builder.Services.AddScoped<IUserPreferenceRepo, UserPreferenceRepo>();

// -------------------------------------------------------
// Background Workers
// -------------------------------------------------------
builder.Services.AddHostedService<EventConsumer>();
builder.Services.AddHostedService<InAppDeliveryWorker>();
builder.Services.AddHostedService<EmailDeliveryWorker>();

// -------------------------------------------------------
// Health Checks
// -------------------------------------------------------
builder.Services.AddHealthChecks().AddDbContextCheck<NotificationDbContext>("database");

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

app.UseGlobalExceptionMiddleware();
app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(200 * retryAttempt));
}