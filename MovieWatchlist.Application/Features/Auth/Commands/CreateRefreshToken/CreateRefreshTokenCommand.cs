using MediatR;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.CreateRefreshToken;

public record CreateRefreshTokenCommand(
    int UserId
) : IRequest<Result<RefreshTokenResult>>;

