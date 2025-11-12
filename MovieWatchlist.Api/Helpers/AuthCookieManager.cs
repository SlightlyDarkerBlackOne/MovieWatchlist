using Microsoft.Extensions.Options;
using MovieWatchlist.Api.Constants;
using MovieWatchlist.Api.Options;
using MovieWatchlist.Application.Features.Auth.Common;

namespace MovieWatchlist.Api.Helpers;

public interface IAuthCookieManager
{
    void SetAuthCookies(HttpResponse response, AuthenticationResult auth);
    void ClearAuthCookies(HttpResponse response);
}

public class AuthCookieManager : IAuthCookieManager
{
    private readonly AuthCookieOptions _options;
    private readonly IWebHostEnvironment _environment;

    public AuthCookieManager(IOptions<AuthCookieOptions> options, IWebHostEnvironment environment)
    {
        _options = options.Value;
        _environment = environment;
    }

    public void SetAuthCookies(HttpResponse response, AuthenticationResult auth)
    {
        if (auth.ExpiresAt == null)
            return;

        var expiresAt = auth.ExpiresAt.Value;
        var cookieOptions = new CookieOptions
        {
            HttpOnly = _options.HttpOnly,
            Secure = _options.Secure || (!_environment.IsDevelopment() && !_environment.IsEnvironment("Testing")),
            SameSite = ParseSameSiteMode(_options.SameSite),
            Expires = expiresAt
        };

        var accessTokenName = !string.IsNullOrEmpty(_options.AccessTokenName) 
            ? _options.AccessTokenName 
            : CookieNames.AccessToken;
        var refreshTokenName = !string.IsNullOrEmpty(_options.RefreshTokenName) 
            ? _options.RefreshTokenName 
            : CookieNames.RefreshToken;

        if (!string.IsNullOrEmpty(auth.Token))
        {
            response.Cookies.Append(accessTokenName, auth.Token, cookieOptions);
        }

        if (!string.IsNullOrEmpty(auth.RefreshToken))
        {
            response.Cookies.Append(refreshTokenName, auth.RefreshToken, cookieOptions);
        }
    }

    public void ClearAuthCookies(HttpResponse response)
    {
        var accessTokenName = !string.IsNullOrEmpty(_options.AccessTokenName) 
            ? _options.AccessTokenName 
            : CookieNames.AccessToken;
        var refreshTokenName = !string.IsNullOrEmpty(_options.RefreshTokenName) 
            ? _options.RefreshTokenName 
            : CookieNames.RefreshToken;

        response.Cookies.Delete(accessTokenName);
        response.Cookies.Delete(refreshTokenName);
    }

    private static SameSiteMode ParseSameSiteMode(string sameSite)
    {
        return sameSite?.ToLowerInvariant() switch
        {
            "strict" => SameSiteMode.Strict,
            "lax" => SameSiteMode.Lax,
            "none" => SameSiteMode.None,
            _ => SameSiteMode.Lax
        };
    }
}

