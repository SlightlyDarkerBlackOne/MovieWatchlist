namespace MovieWatchlist.Application.Features.Auth.Commands.ForgotPassword;

public record PasswordResetResponse(
    bool Success,
    string Message
);
