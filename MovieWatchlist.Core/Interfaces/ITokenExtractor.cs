namespace MovieWatchlist.Core.Interfaces;

public interface ITokenExtractor
{
    string? ExtractTokenFromHeader();
    string? ExtractTokenFromCookie(string cookieName);
}

