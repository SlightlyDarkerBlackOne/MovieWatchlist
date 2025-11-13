using MediatR;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;
using Mapster;

namespace MovieWatchlist.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IAuthenticationService _authService;
    private readonly IAuthCookieService _cookieService;

    public LoginCommandHandler(
        IAuthenticationService authService,
        IAuthCookieService cookieService)
    {
        _authService = authService;
        _cookieService = cookieService;
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var authResult = await _authService.LoginAsync(request);
        
        if (authResult.IsFailure)
            return Result<LoginResponse>.Failure(authResult.Error);

        _cookieService.SetAuthCookies(authResult.Value!.Token, authResult.Value!.RefreshToken, authResult.Value!.ExpiresAt);
        
        var response = authResult.Value!.Adapt<LoginResponse>();
        return Result<LoginResponse>.Success(response);
    }
}

