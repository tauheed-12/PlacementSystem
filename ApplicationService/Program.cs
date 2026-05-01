using ApplicationService.Data;
using ApplicationService.HttpClients;
using ApplicationService.HttpClients.Interfaces;
using Common.Contracts.Infrastructure;
using ApplicationService.Repositories;
using ApplicationService.Repositories.Interfaces;
using Common.Contracts.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddOptions<ServiceEndpointOptions>("PlacementDriveService")
    .Bind(builder.Configuration.GetSection("Services:PlacementDriveService"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpClient<IPlacementDriveServiceClient>((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptionsMonitor<ServiceEndpointOptions>>().Get("PlacementDriveService");
    client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<RequestContextAccessor>();

builder.Services.AddScoped<ApplicationService.Data.Interfaces.IUnitOfWork, ApplicationService.Data.UnitOfWork>();
builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<ApplicationService.Services.Interfaces.IApplicationService, ApplicationService.Services.ApplicationService>();
builder.Services.AddScoped<IPlacementDriveServiceClient, PlacementDriveServiceClient>();

builder.Services.AddControllers();
builder.Services.AddAuthorization();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ApplicationService API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<LocalGatewaySimulationMiddleware>();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseMiddleware<ApplicationService.Middleware.GlobalExceptionMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();