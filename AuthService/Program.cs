using AuthService.Data;
using AuthService.Interfaces;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application services and infrastructure BEFORE building the app
Console.WriteLine(builder.Configuration.GetConnectionString("AuthDb"));
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(
    "Server=host.docker.internal,1433;" +
    "Database=AuthDb;" +
    "User Id=docker_user;" +
    "Password=Strong@123;" +
    "TrustServerCertificate=True;"
));

builder.Services.AddScoped<TokenService>(); // Register TokenService for dependency injection
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)
            )
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddScoped<IEmailService, EmailService>(); // Register EmailService for dependency injection

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Authentication must run before Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
