using DashboardOrchestrationService.Clients;
using DashboardOrchestrationService.Clients.Interfaces;
using Common.Contracts.Configuration;
using Common.Contracts.Infrastructure;
using DashboardOrchestrationService.Middleware;
using DashboardOrchestrationService.Services;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IStudentDashboardService, StudentDashboardService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<RequestContextAccessor>();

builder.Services.AddOptions<ServiceEndpointOptions>("StudentService")
    .Bind(builder.Configuration.GetSection("Services:StudentService"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<ServiceEndpointOptions>("ApplicationService")
    .Bind(builder.Configuration.GetSection("Services:ApplicationService"))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<ServiceEndpointOptions>("PlacementDriveService")
    .Bind(builder.Configuration.GetSection("Services:PlacementDriveService"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<IStudentServiceClient, StudentServiceClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptionsMonitor<ServiceEndpointOptions>>().Get("StudentService");
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
})
.AddPolicyHandler(GetRetryPolicy());

builder.Services.AddHttpClient<IApplicationServiceClient, ApplicationServiceClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptionsMonitor<ServiceEndpointOptions>>().Get("ApplicationService");
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
})
.AddPolicyHandler(GetRetryPolicy());

builder.Services.AddHttpClient<IPlacementDriveServiceClient, PlacementDriveServiceClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptionsMonitor<ServiceEndpointOptions>>().Get("PlacementDriveService");
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
})
.AddPolicyHandler(GetRetryPolicy());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseGlobalExceptionMiddleware();

app.MapControllers();

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