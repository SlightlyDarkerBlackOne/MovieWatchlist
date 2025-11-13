using System.Net;
using System.Text.Json;
using MovieWatchlist.Api.Constants;
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
        // Skip exception handling for OPTIONS preflight requests
        if (context.Request.Method == MiddlewareConstants.HTTP_METHOD_OPTIONS)
        {
            await _next(context);
            return;
        }
        
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, MiddlewareConstants.ERROR_MESSAGE_UNHANDLED_EXCEPTION);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = MiddlewareConstants.CONTENT_TYPE_JSON;

        object response = exception switch
        {
            ValidationException validationEx => new
            {
                statusCode = (int)HttpStatusCode.BadRequest,
                error = new
                {
                    code = MiddlewareConstants.ERROR_CODE_VALIDATION_ERROR,
                    message = validationEx.Message,
                    details = validationEx.Details,
                    timestamp = DateTime.UtcNow
                }
            },
            UnauthorizedAccessException => new
            {
                statusCode = (int)HttpStatusCode.Unauthorized,
                error = new
                {
                    code = MiddlewareConstants.ERROR_CODE_UNAUTHORIZED,
                    message = MiddlewareConstants.ERROR_MESSAGE_AUTHENTICATION_REQUIRED,
                    timestamp = DateTime.UtcNow
                }
            },
            ArgumentException => new
            {
                statusCode = (int)HttpStatusCode.BadRequest,
                error = new
                {
                    code = MiddlewareConstants.ERROR_CODE_BAD_REQUEST,
                    message = MiddlewareConstants.ERROR_MESSAGE_INVALID_REQUEST_PARAMETERS,
                    timestamp = DateTime.UtcNow
                }
            },
            RateLimitException rateLimitEx => new
            {
                statusCode = (int)HttpStatusCode.TooManyRequests,
                error = new
                {
                    code = rateLimitEx.ErrorCode,
                    message = rateLimitEx.Message,
                    details = rateLimitEx.Details,
                    timestamp = DateTime.UtcNow
                }
            },
            ExternalServiceException externalEx => new
            {
                statusCode = externalEx.StatusCode,
                error = new
                {
                    code = externalEx.ErrorCode,
                    message = externalEx.Message,
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
                statusCode = (int)HttpStatusCode.InternalServerError,
                error = new
                {
                    code = MiddlewareConstants.ERROR_CODE_INTERNAL_SERVER_ERROR,
                    message = MiddlewareConstants.ERROR_MESSAGE_UNEXPECTED_ERROR,
                    timestamp = DateTime.UtcNow
                }
            }
        };

        var statusCode = response.GetType().GetProperty(MiddlewareConstants.PROPERTY_STATUS_CODE)?.GetValue(response) as int? ?? (int)HttpStatusCode.InternalServerError;
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
