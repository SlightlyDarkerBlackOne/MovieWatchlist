namespace MovieWatchlist.Application.Features.Auth.Common;

public record AuthenticationResult(
    bool IsSuccess,
    string? Token = null,
    string? RefreshToken = null,
    DateTime? ExpiresAt = null,
    string? ErrorMessage = null,
    UserInfo? User = null
);

