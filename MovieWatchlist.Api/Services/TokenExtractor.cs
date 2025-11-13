using Microsoft.AspNetCore.Http;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Api.Services;

public class TokenExtractor : ITokenExtractor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenExtractor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? ExtractTokenFromHeader()
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true)
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }
        return null;
    }

    public string? ExtractTokenFromCookie(string cookieName)
    {
        return _httpContextAccessor.HttpContext?.Request.Cookies[cookieName];
    }
}

