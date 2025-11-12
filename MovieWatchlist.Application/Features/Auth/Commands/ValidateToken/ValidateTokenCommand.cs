using MediatR;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.ValidateToken;

public record ValidateTokenCommand(
    string Token
) : IRequest<Result<bool>>;

