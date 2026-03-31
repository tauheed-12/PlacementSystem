using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// Configuration
// ----------------------------------------------------
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// ----------------------------------------------------
// Authentication
// ----------------------------------------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

builder.Services.AddAuthorization();

// ----------------------------------------------------
// Rate Limiter (based on JWT claim)
// ----------------------------------------------------
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var userId = context.User?.FindFirst("sub")?.Value ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(userId, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ----------------------------------------------------
// Reverse Proxy (ONLY ONCE)
// ----------------------------------------------------
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(async context =>
        {
            var user = context.HttpContext.User;

            // Remove any incoming spoofed headers
            context.ProxyRequest.Headers.Remove("X-User-Id");
            context.ProxyRequest.Headers.Remove("X-User-Email");
            context.ProxyRequest.Headers.Remove("X-User-Role");

            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst("sub")?.Value;
                var email = user.FindFirst("email")?.Value;
                var roles = user.FindAll("role").Select(c => c.Value);

                if (!string.IsNullOrEmpty(userId))
                    context.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Id", userId);

                if (!string.IsNullOrEmpty(email))
                    context.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Email", email);

                if (roles.Any())
                    context.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Role", string.Join(",", roles));
            }
        });
    });

// ----------------------------------------------------
// Swagger (optional, for testing)
// ----------------------------------------------------
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API Gateway",
        Version = "v1"
    });

    //Add JWT Bearer definition
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer <your_token>"
    });

    //Apply JWT globally
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ----------------------------------------------------
// Build
// ----------------------------------------------------
var app = builder.Build();

// ----------------------------------------------------
// Middleware
// ----------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/auth/swagger/v1/swagger.json", "Auth Service");
        c.SwaggerEndpoint("/drives/swagger/v1/swagger.json", "Drives Service");
        c.SwaggerEndpoint("/dashboard/swagger/v1/swagger.json", "Dashboard Service");
        // c.SwaggerEndpoint("/student/swagger/v1/swagger.json", "Student Service");
        // c.SwaggerEndpoint("/application/swagger/v1/swagger.json", "Application Service");
        // c.SwaggerEndpoint("/notification/swagger/v1/swagger.json", "Notification Service");
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapReverseProxy();

app.Run();