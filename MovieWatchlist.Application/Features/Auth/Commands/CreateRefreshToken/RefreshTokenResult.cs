namespace MovieWatchlist.Application.Features.Auth.Commands.CreateRefreshToken;

public record RefreshTokenResult(
    string Token,
    DateTime ExpiresAt
);

