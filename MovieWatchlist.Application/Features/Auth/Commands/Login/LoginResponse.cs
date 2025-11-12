using MovieWatchlist.Application.Features.Auth.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.Login;

public record LoginResponse(
    UserInfo User,
    DateTime ExpiresAt
);

