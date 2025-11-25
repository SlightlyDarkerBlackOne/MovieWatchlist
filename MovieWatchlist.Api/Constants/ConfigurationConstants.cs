namespace MovieWatchlist.Api.Constants;

public static class ConfigurationConstants
{
    // Configuration Section Paths
    public const string TMDB_SETTINGS_API_KEY = "TmdbSettings:ApiKey";
    public const string TMDB_SETTINGS_BASE_URL = "TmdbSettings:BaseUrl";
    public const string TMDB_SETTINGS_IMAGE_BASE_URL = "TmdbSettings:ImageBaseUrl";
    public const string JWT_SETTINGS_SECRET_KEY = "JwtSettings:SecretKey";
    public const string JWT_SETTINGS_ISSUER = "JwtSettings:Issuer";
    public const string JWT_SETTINGS_AUDIENCE = "JwtSettings:Audience";
    public const string JWT_SETTINGS_EXPIRATION_MINUTES = "JwtSettings:ExpirationMinutes";
    public const string JWT_SETTINGS_REFRESH_TOKEN_EXPIRATION_DAYS = "JwtSettings:RefreshTokenExpirationDays";
    public const string AUTH_COOKIE_SETTINGS = "AuthCookieSettings";
    public const string DEFAULT_CONNECTION_STRING = "DefaultConnection";
    
    // Default Values
    public const string DEFAULT_TMDB_BASE_URL = "https://api.themoviedb.org/3";
    public const string DEFAULT_TMDB_IMAGE_BASE_URL = "https://image.tmdb.org/t/p";
    public const string DEFAULT_FRONTEND_URL = "http://localhost:3000";
    public const string PRODUCTION_FRONTEND_URL = "https://moviewatchlist-frontend-nkol.onrender.com";
    
    // CORS Settings
    public const int CORS_PREFLIGHT_CACHE_HOURS = 24;
    
    // Rate Limiting Settings
    public const int RATE_LIMIT_DEVELOPMENT_MAX_REQUESTS = 100;
    public const int RATE_LIMIT_PRODUCTION_MAX_REQUESTS = 60;
    public const int RATE_LIMIT_TIME_WINDOW_MINUTES = 1;
}
