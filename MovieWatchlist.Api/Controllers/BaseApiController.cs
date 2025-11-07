using Microsoft.AspNetCore.Mvc;

namespace MovieWatchlist.Api.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected string? GetTokenFromHeader()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        if (authHeader?.StartsWith("Bearer ") == true)
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }
        return null;
    }

    protected string? GetTokenFromCookie(string cookieName)
    {
        return Request.Cookies[cookieName];
    }
}

