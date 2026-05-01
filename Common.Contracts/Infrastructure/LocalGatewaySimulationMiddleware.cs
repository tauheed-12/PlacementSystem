using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class LocalGatewaySimulationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LocalGatewaySimulationMiddleware> _logger;

    public LocalGatewaySimulationMiddleware(RequestDelegate next, ILogger<LocalGatewaySimulationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        _logger.LogInformation("Simulating API Gateway - Processing request: {Method} {Path}",
            context.Request.Method, context.Request.Path);
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length);

            var handler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = handler.ReadJwtToken(token);
                _logger.LogInformation("Simulating API Gateway - Extracted JWT claims: {Claims}",
                    string.Join(", ", jwtToken.Claims.Select(c => $"{c.Type}={c.Value}")));
                    
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                var email  = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
                var roles  = jwtToken.Claims.Where(c => c.Type == "role").Select(c => c.Value);

                _logger.LogInformation("Simulating API Gateway - UserId: {UserId}, Email: {Email}, Roles: {Roles}",
                    userId, email, string.Join(",", roles));

                if (!string.IsNullOrEmpty(userId))
                    context.Request.Headers["X-User-Id"] = userId;

                if (!string.IsNullOrEmpty(email))
                    context.Request.Headers["X-User-Email"] = email;

                if (roles.Any())
                    context.Request.Headers["X-User-Role"] = string.Join(",", roles);
            }
            catch
            {
                _logger.LogWarning("Simulating API Gateway - Invalid JWT token provided");  
            }
        }

        await _next(context);
    }
}