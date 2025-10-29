using Microsoft.Extensions.Logging;

namespace MovieWatchlist.Application.Services;

public interface IRetryPolicyService
{
    Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int baseDelayMs = 1000,
        string? operationName = null);
}

public class RetryPolicyService : IRetryPolicyService
{
    private readonly ILogger<RetryPolicyService> _logger;

    public RetryPolicyService(ILogger<RetryPolicyService> logger)
    {
        _logger = logger;
    }

    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int baseDelayMs = 1000,
        string? operationName = null)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                if (attempt == maxRetries - 1)
                {
                    _logger.LogError(ex, 
                        "Operation {OperationName} failed after {MaxRetries} attempts", 
                        operationName ?? "Unknown", 
                        maxRetries);
                    throw;
                }

                var delay = baseDelayMs * (int)Math.Pow(2, attempt);
                _logger.LogWarning(ex, 
                    "Operation {OperationName} failed, attempt {Attempt}/{MaxRetries}. Retrying in {Delay}ms", 
                    operationName ?? "Unknown", 
                    attempt + 1, 
                    maxRetries, 
                    delay);

                await Task.Delay(delay);
            }
        }

        throw new InvalidOperationException("Retry logic should never reach this point");
    }
}

