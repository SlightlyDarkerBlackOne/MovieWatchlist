namespace MovieWatchlist.Core.Constants;

/// <summary>
/// Centralized error and validation messages to maintain consistency and enable easy localization
/// </summary>
public static class ErrorMessages
{
    #region Authentication Errors
    
    public const string EmailAlreadyRegistered = "Email is already registered";
    public const string UsernameAlreadyTaken = "Username is already taken";
    public const string InvalidCredentials = "Invalid username/email or password";
    public const string InvalidEmailFormat = "Invalid email format";
    public const string InvalidPasswordFormat = "Invalid password format";
    public const string InvalidModelState = "Invalid model state";
    public const string ValidationFailed = "Validation failed";
    public const string RegistrationFailed = "Registration failed";
    public const string LogoutFailed = "Logout failed";
    public const string TokenNotProvided = "Token not provided";
    public const string RefreshTokenNotProvided = "Refresh token not provided";
    public const string InvalidToken = "Invalid token";
    public const string ResetTokenRequired = "Reset token is required";
    public const string PasswordMismatch = "New password and confirm password do not match";
    
    #endregion

    #region Password Reset Errors
    
    public const string InvalidOrExpiredResetToken = "Invalid or expired reset token.";
    public const string InvalidOrExpiredRefreshToken = "Invalid or expired refresh token.";
    public const string UserNotAuthenticated = "User not authenticated";
    public const string UserNotFound = "User not found";
    public const string PasswordResetSuccess = "Password has been reset successfully.";
    public const string PasswordResetFailed = "An error occurred while resetting your password.";
    public const string PasswordResetRequestError = "An error occurred while processing your request.";
    public const string PasswordResetEmailSent = "If the email exists, a password reset link has been sent.";
    
    #endregion

    #region Watchlist Errors
    
    public const string MovieNotFound = "Movie not found";
    public const string MovieAlreadyInWatchlist = "Movie is already in user's watchlist";
    public const string WatchlistItemNotFound = "Watchlist item not found";
    public const string SearchQueryRequired = "Search query is required";
    
    #endregion

    #region TMDB/Movie Errors
    
    public const string TmdbRateLimitExceeded = "TMDB API rate limit exceeded. Please try again in a moment.";
    public const string FailedToFetchMovieData = "Failed to fetch movie data";
    public const string MovieWithTmdbIdNotFound = "Movie with TMDB ID {0} not found";
    public const string GenreRequired = "Genre is required.";
    
    #endregion

    #region Generic Error Messages
    
    public const string UnexpectedError = "An unexpected error occurred";
    
    #endregion

    #region Validation Messages
    
    public static string UsernameValidation => ValidationConstants.Username.InvalidFormatMessage;
    public static string EmailValidation => ValidationConstants.Email.InvalidFormatMessage;
    public static string PasswordValidation => ValidationConstants.Password.InvalidFormatMessage;
    
    #endregion
}
