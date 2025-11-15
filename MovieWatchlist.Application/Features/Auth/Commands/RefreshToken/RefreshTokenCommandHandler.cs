using Mapster;
using MediatR;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IAuthenticationService _authService;
    private readonly ITokenExtractor _tokenExtractor;
    private readonly IAuthCookieService _cookieService;

    public RefreshTokenCommandHandler(
        IAuthenticationService authService,
        ITokenExtractor tokenExtractor,
        IAuthCookieService cookieService)
    {
        _authService = authService;
        _tokenExtractor = tokenExtractor;
        _cookieService = cookieService;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var refreshToken = _tokenExtractor.ExtractTokenFromCookie(CookieNames.RefreshToken);
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Result<RefreshTokenResponse>.Failure(ErrorMessages.RefreshTokenNotProvided);
        }

        var authResult = await _authService.RefreshTokenAsync(refreshToken);
        
        _cookieService.SetAuthCookies(authResult.Token, authResult.RefreshToken, authResult.ExpiresAt);
        
        var response = authResult.Adapt<RefreshTokenResponse>();
        return Result<RefreshTokenResponse>.Success(response);
    }
}

