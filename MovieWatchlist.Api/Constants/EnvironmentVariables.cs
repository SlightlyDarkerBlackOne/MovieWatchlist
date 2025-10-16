namespace MovieWatchlist.Api.Constants;

public static class EnvironmentVariables
{
    // TMDB Configuration
    public const string TMDB_API_KEY = "TMDB_API_KEY";
    public const string TMDB_BASE_URL = "TMDB_BASE_URL";
    public const string TMDB_IMAGE_BASE_URL = "TMDB_IMAGE_BASE_URL";
    
    // Database Configuration
    public const string DATABASE_CONNECTION_STRING = "DATABASE_CONNECTION_STRING";
    
    // JWT Configuration
    public const string JWT_SECRET_KEY = "JWT_SECRET_KEY";
    public const string JWT_ISSUER = "JWT_ISSUER";
    public const string JWT_AUDIENCE = "JWT_AUDIENCE";
    public const string JWT_EXPIRATION_MINUTES = "JWT_EXPIRATION_MINUTES";
    public const string JWT_REFRESH_DAYS = "JWT_REFRESH_DAYS";
}
