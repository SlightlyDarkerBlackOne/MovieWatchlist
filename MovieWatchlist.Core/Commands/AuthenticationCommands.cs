namespace MovieWatchlist.Core.Commands;

public record RegisterCommand(
    string Username,
    string Email,
    string Password
);

public record LoginCommand(
    string UsernameOrEmail,
    string Password
);

public record ForgotPasswordCommand(
    string Email
);

public record ResetPasswordCommand(
    string Token,
    string NewPassword
);

public record AuthenticationResult(
    bool IsSuccess,
    string? Token = null,
    string? RefreshToken = null,
    DateTime? ExpiresAt = null,
    string? ErrorMessage = null,
    UserInfo? User = null
);

public record UserInfo(
    int Id,
    string Username,
    string Email,
    DateTime CreatedAt
);

public record PasswordResetResponse(
    bool Success,
    string Message
);


