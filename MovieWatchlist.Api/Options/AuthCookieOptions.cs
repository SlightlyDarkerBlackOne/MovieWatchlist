namespace MovieWatchlist.Api.Options;

public class AuthCookieOptions
{
    public bool HttpOnly { get; set; } = true;
    public bool Secure { get; set; } = false;
    public string SameSite { get; set; } = "Lax";
    public string AccessTokenName { get; set; } = "accessToken";
    public string RefreshTokenName { get; set; } = "refreshToken";
}

