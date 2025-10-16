namespace MovieWatchlist.Core.Exceptions;

public class ApiException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public object? Details { get; }

    public ApiException(string message, int statusCode = 500, string errorCode = "INTERNAL_ERROR", object? details = null) 
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Details = details;
    }
}

public class ValidationException : ApiException
{
    public ValidationException(string message, object? details = null) 
        : base(message, 400, "VALIDATION_ERROR", details)
    {
    }
}

public class AuthenticationException : ApiException
{
    public AuthenticationException(string message) 
        : base(message, 401, "AUTHENTICATION_ERROR")
    {
    }
}

public class AuthorizationException : ApiException
{
    public AuthorizationException(string message) 
        : base(message, 403, "AUTHORIZATION_ERROR")
    {
    }
}

public class NotFoundException : ApiException
{
    public NotFoundException(string message) 
        : base(message, 404, "NOT_FOUND")
    {
    }
}

public class ConflictException : ApiException
{
    public ConflictException(string message) 
        : base(message, 409, "CONFLICT")
    {
    }
}

public class RateLimitException : ApiException
{
    public RateLimitException(string message, int retryAfterSeconds) 
        : base(message, 429, "RATE_LIMIT_EXCEEDED", new { retryAfterSeconds })
    {
    }
}


