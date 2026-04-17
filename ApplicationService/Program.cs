using ApplicationService.Data;
using ApplicationService.HttpClients;
using ApplicationService.HttpClients.Interfaces;
using Common.Contracts.Infrastructure;
using ApplicationService.Repositories;
using ApplicationService.Repositories.Interfaces;
using Common.Contracts.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using Microsoft.OpenApi.Models;
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
builder.Services.AddOptions<ServiceEndpointOptions>("PlacementDriveService")
    .Bind(builder.Configuration.GetSection("Services:PlacementDriveService"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<IPlacementDriveServiceClient>((sp, client) =>
{
    var placementDriveOptions = sp.GetRequiredService<IOptionsMonitor<ServiceEndpointOptions>>().Get("PlacementDriveService");
    var placementDriveBaseUrl = placementDriveOptions.BaseUrl;
    client.BaseAddress = new Uri(placementDriveBaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(10);
})
.AddPolicyHandler(GetRetryPolicy());

// -------------------------------------------------------
// HTTP Context Accessor
// Needed to read X-User-Id and X-User-Role headers
// forwarded by the API Gateway
// -------------------------------------------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<RequestContextAccessor>();

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
// Swagger
// No JWT security definition needed ù auth is handled
// at the API Gateway, not here
// -------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ApplicationService API",
        Version = "v1"
    });

    // Optional ù only if you want to manually pass headers in Swagger during dev testing
    options.AddSecurityDefinition("GatewayHeaders", new OpenApiSecurityScheme
    {
        Name = "X-User-Id",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Paste a user GUID to simulate gateway forwarding"
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