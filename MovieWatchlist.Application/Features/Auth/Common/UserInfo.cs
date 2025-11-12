namespace MovieWatchlist.Application.Features.Auth.Common;

public record UserInfo(
    int Id,
    string Username,
    string Email,
    DateTime CreatedAt
);

