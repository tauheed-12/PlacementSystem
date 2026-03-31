using System.Net;
using System.Text.Json;
using ApplicationService.Exceptions;

namespace ApplicationService.Middleware
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

            var json = JsonSerializer.Serialize(response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            await context.Response.WriteAsync(json);
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ValidationException => (int)HttpStatusCode.BadRequest,        // 400
                NotFoundException => (int)HttpStatusCode.NotFound,           // 404
                ConflictException => (int)HttpStatusCode.Conflict,           // 409
                UnauthorizedAccessException => (int)HttpStatusCode.Forbidden,// 403
                _ => (int)HttpStatusCode.InternalServerError                 // 500
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