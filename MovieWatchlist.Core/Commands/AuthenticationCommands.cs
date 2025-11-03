using MediatR;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Core.Commands;

public record RegisterCommand(
    string Username,
    string Email,
    string Password
) : IRequest<Result<AuthenticationResult>>;

public record LoginCommand(
    string UsernameOrEmail,
    string Password
) : IRequest<Result<AuthenticationResult>>;

public record CreateRefreshTokenCommand(
    int UserId
) : IRequest<Result<RefreshTokenResult>>;

public record ForgotPasswordCommand(
    string Email
) : IRequest<Result<PasswordResetResponse>>;

public record ResetPasswordCommand(
    string Token,
    string NewPassword
) : IRequest<Result<PasswordResetResponse>>;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<Result<AuthenticationResult>>;

public record LogoutCommand(
    string Token
) : IRequest<Result<bool>>;

public record ValidateTokenCommand(
    string Token
) : IRequest<Result<bool>>;

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

public record RefreshTokenResult(
    string Token,
    DateTime ExpiresAt
);


