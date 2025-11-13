namespace MovieWatchlist.Core.Interfaces;

public interface IAuthCookieService
{
    void SetAuthCookies(string? token, string? refreshToken, DateTime? expiresAt);
    void ClearAuthCookies();
}

