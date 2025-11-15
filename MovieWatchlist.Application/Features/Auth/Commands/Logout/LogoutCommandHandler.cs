using MediatR;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<LogoutResponse>>
{
    private readonly IAuthenticationService _authService;
    private readonly ITokenExtractor _tokenExtractor;
    private readonly IAuthCookieService _cookieService;

    public LogoutCommandHandler(
        IAuthenticationService authService,
        ITokenExtractor tokenExtractor,
        IAuthCookieService cookieService)
    {
        _authService = authService;
        _tokenExtractor = tokenExtractor;
        _cookieService = cookieService;
    }

    public async Task<Result<LogoutResponse>> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var token = _tokenExtractor.ExtractTokenFromCookie(CookieNames.AccessToken);
        if (!string.IsNullOrEmpty(token))
        {
            var success = await _authService.LogoutAsync(token);
            if (!success)
            {
                return Result<LogoutResponse>.Failure(ErrorMessages.LogoutFailed);
            }
        }

        _cookieService.ClearAuthCookies();

        var response = new LogoutResponse(SuccessMessages.LogoutSuccess);
        return Result<LogoutResponse>.Success(response);
    }
}

