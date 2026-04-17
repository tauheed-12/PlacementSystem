using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Common.Exceptions;

namespace Common.Contracts.Web;

public static class ProblemDetailsWriter
{
    public static async Task WriteAsync(HttpContext context, Exception exception, ILogger logger)
    {
        // Unwrap DbUpdateException to a meaningful domain exception
        if (exception is DbUpdateException dbEx)
            exception = UnwrapDbException(dbEx);

        var statusCode = MapStatusCode(exception);
        var title = ReasonPhrases.GetReasonPhrase(statusCode);
        var details = statusCode >= 500 ? "An unexpected error occurred." : exception.Message;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = details,
            Instance = context.Request.Path
        };

        problem.Extensions["code"] = MapErrorCode(exception);
        problem.Extensions["traceId"] = context.TraceIdentifier;
        problem.Extensions["method"] = context.Request.Method;
        problem.Extensions["timestampUtc"] = DateTime.UtcNow;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        if (statusCode >= 500)
            logger.LogError(exception, "Unhandled exception ({StatusCode}) traceId={TraceId}", statusCode, context.TraceIdentifier);
        else
            logger.LogWarning(exception, "Handled exception ({StatusCode}) traceId={TraceId}", statusCode, context.TraceIdentifier);

        await context.Response.WriteAsJsonAsync(problem);
    }

    private static Exception UnwrapDbException(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;

        // Unique constraint violations ? 409
        if (message.Contains("UQ_") || message.Contains("unique"))
            return new ConflictException("A record with the same value already exists.");

        // Check constraint violations ? 400
        if (message.Contains("CK_") || message.Contains("Cannot insert") || message.Contains("CHECK constraint"))
            return new BadRequestException(ResolveCheckConstraintMessage(message));

        // Anything else is a real 500
        return ex;
    }

    private static string ResolveCheckConstraintMessage(string message) => message switch
    {
        _ when message.Contains("Package_Positive") => "Package must be greater than 0.",
        _ when message.Contains("Deadline_Before_DriveDate") => "Application deadline must be before the drive date.",
        _ when message.Contains("DriveDate_Future") => "Drive date must be in the future.",
        _ when message.Contains("Cannot insert") => "Invalid data provided.",
        _ => "The request violates a data constraint."
    };

    private static int MapStatusCode(Exception exception)
    {
        if (exception is UnauthorizedAccessException) return StatusCodes.Status401Unauthorized;
        if (exception is KeyNotFoundException) return StatusCodes.Status404NotFound;
        if (exception is ArgumentException
         or ArgumentNullException) return StatusCodes.Status400BadRequest;

        return exception.GetType().Name switch
        {
            "ValidationException" => StatusCodes.Status400BadRequest,
            "BadRequestException" => StatusCodes.Status400BadRequest,
            "UnauthorizedException" => StatusCodes.Status401Unauthorized,
            "ForbiddenException" => StatusCodes.Status403Forbidden,
            "NotFoundException" => StatusCodes.Status404NotFound,
            "ConflictException" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string MapErrorCode(Exception exception)
    {
        if (exception is UnauthorizedAccessException) return ErrorCodes.Unauthorized;
        if (exception is KeyNotFoundException) return ErrorCodes.NotFound;
        if (exception is ArgumentException
         or ArgumentNullException) return ErrorCodes.BadRequest;

        return exception.GetType().Name switch
        {
            "ValidationException" => ErrorCodes.ValidationFailed,
            "BadRequestException" => ErrorCodes.BadRequest,
            "UnauthorizedException" => ErrorCodes.Unauthorized,
            "ForbiddenException" => ErrorCodes.Forbidden,
            "NotFoundException" => ErrorCodes.NotFound,
            "ConflictException" => ErrorCodes.Conflict,
            _ => ErrorCodes.InternalError
        };
    }
}