using MediatR;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Common;
using System;
using System.Text.Json;

namespace MovieWatchlist.Infrastructure.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() 
    { 
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        try
        {
            _logger.LogInformation("Handling {RequestType}: {@Request}", requestName, request);
            
            var response = await next();
            
            if (IsResult(response) && IsFailure(response))
            {
                var error = GetError(response);
                _logger.LogWarning("Request {RequestType} failed: {Error}. Request: {@Request}", requestName, error, request);
            }
            else
            {
                _logger.LogInformation("Successfully handled {RequestType}", requestName);
            }
            
            return response;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt for {RequestType}: {Message}. Request: {@Request}", requestName, ex.Message, request);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {RequestType}. Request: {@Request}", requestName, request);
            throw;
        }
    }

    private static bool IsResult(object? response)
    {
        if (response == null) return false;
        var type = response.GetType();
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>);
    }

    private static bool IsFailure(object? response)
    {
        if (!IsResult(response) || response == null) return false;
        var isFailureProperty = response.GetType().GetProperty("IsFailure");
        return isFailureProperty?.GetValue(response) as bool? ?? false;
    }

    private static string? GetError(object? response)
    {
        if (!IsResult(response) || response == null) return null;
        var errorProperty = response.GetType().GetProperty("Error");
        return errorProperty?.GetValue(response) as string;
    }
}

