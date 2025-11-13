using MovieWatchlist.Application.Features.Auth.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenResponse(
    UserInfo User,
    DateTime ExpiresAt
);

