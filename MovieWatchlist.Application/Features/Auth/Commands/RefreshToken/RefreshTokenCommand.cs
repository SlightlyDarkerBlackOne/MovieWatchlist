using MediatR;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand() : IRequest<Result<RefreshTokenResponse>>;

