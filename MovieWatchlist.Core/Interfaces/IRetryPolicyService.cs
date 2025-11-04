namespace MovieWatchlist.Core.Interfaces;

public interface IRetryPolicyService
{
    Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int baseDelayMs = 1000,
        string? operationName = null);
}

