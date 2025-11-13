using MovieWatchlist.Api.Helpers;
using MovieWatchlist.Application.Features.Auth.Common;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Api.Services;

public class AuthCookieService : IAuthCookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthCookieManager _cookieManager;

    public AuthCookieService(
        IHttpContextAccessor httpContextAccessor,
        IAuthCookieManager cookieManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _cookieManager = cookieManager;
    }

    public void SetAuthCookies(string? token, string? refreshToken, DateTime? expiresAt)
    {
        var response = _httpContextAccessor.HttpContext?.Response;
        if (response != null && expiresAt.HasValue)
        {
            var auth = new AuthenticationResult(
                IsSuccess: true,
                Token: token,
                RefreshToken: refreshToken,
                ExpiresAt: expiresAt
            );
            _cookieManager.SetAuthCookies(response, auth);
        }
    }

    public void ClearAuthCookies()
    {
        var response = _httpContextAccessor.HttpContext?.Response;
        if (response != null)
        {
            _cookieManager.ClearAuthCookies(response);
        }
    }
}

