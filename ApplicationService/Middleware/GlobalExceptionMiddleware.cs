using System.Net;
using System.Text.Json;
using ApplicationService.Exceptions;

namespace ApplicationService.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                var traceId = context.TraceIdentifier;

                _logger.LogError(ex,
                    "Unhandled exception occurred. TraceId: {TraceId} Message: {Message}",
                    traceId,
                    ex.Message);

                await HandleExceptionAsync(context, ex, traceId);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, string traceId)
        {
            var (statusCode, message) = exception switch
            {
                NotFoundException => (HttpStatusCode.NotFound, exception.Message),

                UnauthorizedAccessException =>
                    (HttpStatusCode.Unauthorized, "Unauthorized access."),

                KeyNotFoundException =>
                    (HttpStatusCode.NotFound, exception.Message),

                ArgumentNullException =>
                    (HttpStatusCode.BadRequest, exception.Message),

                ArgumentException =>
                    (HttpStatusCode.BadRequest, exception.Message),

                InvalidOperationException =>
                    (HttpStatusCode.BadRequest, exception.Message),

                _ =>
                    (HttpStatusCode.InternalServerError,
                    "An unexpected error occurred. Please contact support.")
            };

            var response = new
            {
                success = false,
                status = (int)statusCode,
                error = message,
                traceId = traceId,
                timestamp = DateTime.UtcNow
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}