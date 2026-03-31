using DashboardOrchestrationService.Clients;
using DashboardOrchestrationService.Clients.Interfaces;
using DashboardOrchestrationService.Middleware;
using DashboardOrchestrationService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IStudentDashboardService, StudentDashboardService>();

builder.Services.AddHttpClient<IStudentServiceClient, StudentServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:StudentService"]!);
});

builder.Services.AddHttpClient<IApplicationServiceClient, ApplicationServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ApplicationService"]!);
});

builder.Services.AddHttpClient<IPlacementDriveServiceClient, PlacementDriveServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:PlacementDriveService"]!);
});

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