using MediatR;
using MovieWatchlist.Application.Features.Auth.Common;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string UsernameOrEmail,
    string Password
) : IRequest<Result<AuthenticationResult>>;

