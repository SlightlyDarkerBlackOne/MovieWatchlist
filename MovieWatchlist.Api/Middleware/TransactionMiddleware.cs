using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Api.Middleware;

public class TransactionMiddleware
{
    private readonly RequestDelegate m_next;
    private readonly ILogger<TransactionMiddleware> m_logger;

    public TransactionMiddleware(RequestDelegate next, ILogger<TransactionMiddleware> logger)
    {
        m_next = next;
        m_logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        await m_next(context);

        if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            var changes = await unitOfWork.SaveChangesAsync();
            
            if (changes > 0)
            {
                m_logger.LogDebug("Saved {Changes} changes to database for request {Method} {Path}", 
                    changes, context.Request.Method, context.Request.Path);
            }
        }
        else
        {
            m_logger.LogDebug("Skipping SaveChanges for non-successful response: {StatusCode}", 
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

