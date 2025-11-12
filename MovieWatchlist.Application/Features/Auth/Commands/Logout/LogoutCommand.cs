using MediatR;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.Logout;

public record LogoutCommand(
    string Token
) : IRequest<Result<bool>>;

