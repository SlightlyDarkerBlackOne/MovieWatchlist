using MediatR;
using MovieWatchlist.Application.Commands;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Handlers.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthenticationResult>>
{
    private readonly IAuthenticationService _authService;

    public LoginCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<Result<AuthenticationResult>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request);
    }
}


