using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Api.Middleware;

public class TransactionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TransactionMiddleware> _logger;

    public TransactionMiddleware(RequestDelegate next, ILogger<TransactionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        await _next(context);

        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            var changes = await unitOfWork.SaveChangesAsync();
            
            if (changes > 0)
            {
                _logger.LogDebug("Saved {Changes} changes to database for request {Method} {Path}", 
                    changes, context.Request.Method, context.Request.Path);
            }
        }
        else
        {
            _logger.LogDebug("Skipping SaveChanges for non-successful response: {StatusCode}", 
                context.Response.StatusCode);
        }
    }
}

public static class TransactionMiddlewareExtensions
{
    public static IApplicationBuilder UseTransactionPerRequest(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TransactionMiddleware>();
    }
}

