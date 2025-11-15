using MediatR;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Exceptions;

namespace MovieWatchlist.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior that intercepts Result failures and converts them to appropriate exceptions.
/// This enables centralized error handling through GlobalExceptionMiddleware without requiring controllers
/// to manually check for failures. When a handler returns a failed Result, this behavior maps the error
/// message to the appropriate exception type (AuthenticationException, NotFoundException, etc.) which
/// is then handled by the middleware to return proper HTTP status codes.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ResultFailureBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Processes the request pipeline and converts any Result failures to exceptions.
    /// If the handler returns a failed Result, an appropriate exception is thrown based on the error message.
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (IsResult(response) && IsFailure(response))
        {
            var error = GetError(response);
            ThrowAppropriateException(error);
        }

        return response;
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

    /// <summary>
    /// Maps error messages to appropriate exception types based on error content.
    /// Authentication errors map to AuthenticationException (401), not found errors to NotFoundException (404),
    /// conflicts to ConflictException (409), validation errors to ValidationException (400), and others to ApiException (400).
    /// </summary>
    private static void ThrowAppropriateException(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            throw new ApiException(
                ErrorMessages.UnexpectedError,
                (int)System.Net.HttpStatusCode.InternalServerError,
                ErrorCodes.InternalServerError);
        }

        var errorMessage = error;

        if (errorMessage.Contains(ErrorMessages.InvalidCredentials, StringComparison.OrdinalIgnoreCase) ||
            errorMessage.Contains(ErrorMessages.UserNotAuthenticated, StringComparison.OrdinalIgnoreCase) ||
            errorMessage.Contains(ErrorMessages.TokenNotProvided, StringComparison.OrdinalIgnoreCase) ||
            errorMessage.Contains(ErrorMessages.InvalidToken, StringComparison.OrdinalIgnoreCase) ||
            errorMessage.Contains(ErrorMessages.InvalidOrExpiredRefreshToken, StringComparison.OrdinalIgnoreCase))
        {
            throw new AuthenticationException(errorMessage);
        }

        if (errorMessage.Contains(ErrorMessages.UserNotFound, StringComparison.OrdinalIgnoreCase) ||
            errorMessage.Contains(ErrorMessages.MovieWithTmdbIdNotFound, StringComparison.OrdinalIgnoreCase) ||
            errorMessage.Contains(ErrorMessages.MovieNotFound, StringComparison.OrdinalIgnoreCase) ||
            errorMessage.Contains(ErrorMessages.WatchlistItemNotFound, StringComparison.OrdinalIgnoreCase))
        {
            throw new NotFoundException(errorMessage);
        }

        if (errorMessage.Contains(ErrorMessages.EmailAlreadyRegistered, StringComparison.OrdinalIgnoreCase) ||
            errorMessage.Contains(ErrorMessages.UsernameAlreadyTaken, StringComparison.OrdinalIgnoreCase) ||
            errorMessage.Contains(ErrorMessages.MovieAlreadyInWatchlist, StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException(errorMessage);
        }

        if (errorMessage.Contains(ErrorMessages.ValidationFailed, StringComparison.OrdinalIgnoreCase) ||
            errorMessage.Contains(ErrorMessages.InvalidEmailFormat, StringComparison.OrdinalIgnoreCase) ||
            errorMessage.Contains(ErrorMessages.InvalidPasswordFormat, StringComparison.OrdinalIgnoreCase) ||
            errorMessage.Contains(ErrorMessages.InvalidModelState, StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException(errorMessage);
        }

        throw new ApiException(
            errorMessage,
            (int)System.Net.HttpStatusCode.BadRequest,
            ErrorCodes.BadRequest);
    }
}

