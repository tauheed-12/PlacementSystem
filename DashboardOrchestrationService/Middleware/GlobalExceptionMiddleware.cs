using System.Text.Json;

namespace DashboardOrchestrationService.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = GetStatusCode(exception);
            var response = new ErrorResponse
            {
                StatusCode = statusCode,
                Message = exception.Message,
                Path = context.Request.Path,
                Method = context.Request.Method,
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow
            };
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            // Logging strategy
            if (statusCode >= 500)
            {
                _logger.LogError(exception,
                    "Unhandled exception. StatusCode: {StatusCode}, TraceId: {TraceId}",
                    statusCode, context.TraceIdentifier);
            }
            else
            {
                _logger.LogWarning(exception,
                    "Handled exception. StatusCode: {StatusCode}, TraceId: {TraceId}",
                    statusCode, context.TraceIdentifier);
            }
            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
        private int GetStatusCode(Exception exception)
        {
            // Map specific exceptions to status codes as needed
            return exception switch
            {
                KeyNotFoundException => StatusCodes.Status404NotFound,
                HttpRequestException => StatusCodes.Status503ServiceUnavailable,
                UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };
        }
    }
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
