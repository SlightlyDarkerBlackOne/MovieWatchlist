using MediatR;
using MovieWatchlist.Application.Features.Auth.Common;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken
) : IRequest<Result<AuthenticationResult>>;

