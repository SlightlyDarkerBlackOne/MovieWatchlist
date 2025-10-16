using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Exceptions;

namespace MovieWatchlist.Api.Middleware;

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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        object response = exception switch
        {
            ValidationException validationEx => new
            {
                statusCode = 400,
                error = new
                {
                    code = "VALIDATION_ERROR",
                    message = validationEx.Message,
                    details = validationEx.Details,
                    timestamp = DateTime.UtcNow
                }
            },
            UnauthorizedAccessException => new
            {
                statusCode = 401,
                error = new
                {
                    code = "UNAUTHORIZED",
                    message = "Authentication required",
                    timestamp = DateTime.UtcNow
                }
            },
            ArgumentException => new
            {
                statusCode = 400,
                error = new
                {
                    code = "BAD_REQUEST",
                    message = "Invalid request parameters",
                    timestamp = DateTime.UtcNow
                }
            },
            ApiException apiEx => new
            {
                statusCode = apiEx.StatusCode,
                error = new
                {
                    code = apiEx.ErrorCode,
                    message = apiEx.Message,
                    details = apiEx.Details,
                    timestamp = DateTime.UtcNow
                }
            },
            _ => new
            {
                statusCode = 500,
                error = new
                {
                    code = "INTERNAL_SERVER_ERROR",
                    message = "An unexpected error occurred",
                    timestamp = DateTime.UtcNow
                }
            }
        };

        var statusCode = response.GetType().GetProperty("statusCode")?.GetValue(response) as int? ?? 500;
        context.Response.StatusCode = statusCode;
        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
