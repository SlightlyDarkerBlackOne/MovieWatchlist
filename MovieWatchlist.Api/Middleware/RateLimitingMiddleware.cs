using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace MovieWatchlist.Api.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ConcurrentDictionary<string, RateLimitInfo> _requests = new();
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;

    public RateLimitingMiddleware(RequestDelegate next, int maxRequests = 5, int timeWindowMinutes = 1)
    {
        _next = next;
        _maxRequests = maxRequests;
        _timeWindow = TimeSpan.FromMinutes(timeWindowMinutes);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var now = DateTime.UtcNow;

        var rateLimitInfo = _requests.AddOrUpdate(
            clientId,
            new RateLimitInfo { Count = 1, ResetTime = now.Add(_timeWindow) },
            (key, existing) =>
            {
                if (existing.ResetTime <= now)
                {
                    return new RateLimitInfo { Count = 1, ResetTime = now.Add(_timeWindow) };
                }
                return new RateLimitInfo { Count = existing.Count + 1, ResetTime = existing.ResetTime };
            });

        // Add rate limit headers
        context.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, _maxRequests - rateLimitInfo.Count).ToString();
        context.Response.Headers["X-RateLimit-Reset"] = new DateTimeOffset(rateLimitInfo.ResetTime).ToUnixTimeSeconds().ToString();

        if (rateLimitInfo.Count > _maxRequests)
        {
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                error = "Rate limit exceeded. Please try again later.",
                retryAfter = (int)(rateLimitInfo.ResetTime - now).TotalSeconds
            }));
            return;
        }

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Use IP address and User-Agent for identification
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";
        return $"{ip}:{userAgent}";
    }

    private class RateLimitInfo
    {
        public int Count { get; set; }
        public DateTime ResetTime { get; set; }
    }
}

public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder, int maxRequests = 5, int timeWindowMinutes = 1)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>(maxRequests, timeWindowMinutes);
    }
}
