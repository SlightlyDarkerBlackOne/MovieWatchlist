using MovieWatchlist.Application.Features.Auth.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.Register;

public record RegisterResponse(
    UserInfo User,
    DateTime ExpiresAt
);

