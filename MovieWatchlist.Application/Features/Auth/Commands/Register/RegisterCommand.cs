using MediatR;
using MovieWatchlist.Application.Features.Auth.Common;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Username,
    string Email,
    string Password
) : IRequest<Result<AuthenticationResult>>;

