namespace MovieWatchlist.Tests.Infrastructure;

/// <summary>
/// Common test constants and values
/// </summary>
public static class TestConstants
{
    // Test User Data
    public static class Users
    {
        public const int DefaultUserId = 1;
        public const int SecondUserId = 2;
        public const int NonExistentUserId = 999;
        
        public const string DefaultUsername = "testuser";
        public const string DefaultEmail = "test@example.com";
        public const string DefaultPassword = "TestPassword123!";
        public const string DefaultPasswordHash = "hashed_password_for_testing";
        
        public const string AdminUsername = "admin";
        public const string AdminEmail = "admin@example.com";
        
        public const string InactiveUsername = "inactiveuser";
        public const string InactiveEmail = "inactive@example.com";
    }

    // Test Movie Data
    public static class Movies
    {
        public const int DefaultTmdbId = 12345;
        public const string DefaultTitle = "Test Movie";
        public const string DefaultOverview = "A test movie for unit testing purposes";
        public const double DefaultVoteAverage = 7.5;
        public const int DefaultVoteCount = 1000;
        public const double DefaultPopularity = 50.0;
        public const string DefaultPosterPath = "/test-poster.jpg";
        public const string DefaultBackdropPath = "/test-backdrop.jpg";
    }

    // Test JWT Data
    public static class Jwt
    {
        public const string TestSecretKey = "TestSecretKeyForJWTTokenGeneration123456789";
        public const string TestIssuer = "MovieWatchlistTestAPI";
        public const string TestAudience = "MovieWatchlistTestUsers";
        public const int TestExpirationMinutes = 60;
        public const int TestRefreshTokenExpirationDays = 7;
    }

    // Test TMDB Data
    public static class Tmdb
    {
        public const string TestApiKey = "test_api_key";
        public const string TestImageBaseUrl = "https://image.tmdb.org/t/p";
        public const string TestPosterPath = "/test-poster.jpg";
        public const string TestMovieTitle = "Inception";
        public const string TestMovieOverview = "A mind-bending thriller";
    }

    // Test API Endpoints
    public static class ApiEndpoints
    {
        public const string AuthRegister = "/api/Auth/register";
        public const string AuthLogin = "/api/Auth/login";
        public const string AuthLogout = "/api/Auth/logout";
        public const string AuthRefresh = "/api/Auth/refresh";
        
        public const string MoviesSearch = "/api/Movies/search";
        public const string MoviesPopular = "/api/Movies/popular";
        public const string MoviesDetails = "/api/Movies/{id}";
        public const string MoviesGenre = "/api/Movies/genre/{genre}";
        
        public const string WatchlistUser = "/api/Watchlist/user/{userId}";
        public const string WatchlistAdd = "/api/Watchlist/user/{userId}/add";
        public const string WatchlistItem = "/api/Watchlist/user/{userId}/item/{watchlistItemId}";
        public const string WatchlistStatistics = "/api/Watchlist/user/{userId}/statistics";
        public const string WatchlistFavorites = "/api/Watchlist/user/{userId}/favorites";
        public const string WatchlistRecommendations = "/api/Watchlist/user/{userId}/recommendations";
        public const string WatchlistGenre = "/api/Watchlist/user/{userId}/genre/{genre}";
        public const string WatchlistYearRange = "/api/Watchlist/user/{userId}/year-range";
    }

    // Test HTTP Status Codes
    public static class HttpStatusCodes
    {
        public const int Ok = 200;
        public const int Created = 201;
        public const int BadRequest = 400;
        public const int Unauthorized = 401;
        public const int Forbidden = 403;
        public const int NotFound = 404;
        public const int Conflict = 409;
        public const int InternalServerError = 500;
    }

    // Test Error Messages
    public static class ErrorMessages
    {
        public const string InvalidCredentials = "Invalid username/email or password";
        public const string UserAlreadyExists = "User with this email or username already exists";
        public const string TokenNotProvided = "Token not provided";
        public const string InvalidToken = "Invalid token";
        public const string MovieNotFound = "Movie not found";
        public const string WatchlistItemNotFound = "Watchlist item not found";
        public const string UnauthorizedAccess = "Unauthorized access";
        public const string ValidationFailed = "Validation failed";
    }

    // Test Date Constants
    public static class Dates
    {
        public static readonly DateTime DefaultReleaseDate = new(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime DefaultAddedDate = DateTime.UtcNow.AddDays(-7);
        public static readonly DateTime DefaultWatchedDate = DateTime.UtcNow.AddDays(-3);
        public static readonly DateTime DefaultCreatedAt = DateTime.UtcNow.AddDays(-30);
        public static readonly DateTime DefaultLastLoginAt = DateTime.UtcNow.AddDays(-1);
        public static readonly DateTime DefaultTokenExpiresAt = DateTime.UtcNow.AddDays(7);
    }

    // Test Rating Constants
    public static class Ratings
    {
        public const int MinUserRating = 1;
        public const int MaxUserRating = 10;
        public const int DefaultUserRating = 5;
        public const double DefaultTmdbRating = 7.5;
        public const double HighRating = 8.5;
        public const double LowRating = 4.0;
    }

    // Test Pagination
    public static class Pagination
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
        public const int DefaultPage = 1;
        public const int SecondPage = 2;
        public const int ThirdPage = 3;
        public const int SmallPageSize = 5;
        public const int LargePageSize = 50;
    }

    // Test Collection Indices
    public static class CollectionIndices
    {
        public const int First = 0;
        public const int Second = 1;
        public const int Third = 2;
        public const int Fourth = 3;
    }

    // Test Watchlist Item IDs
    public static class WatchlistItems
    {
        public const int FirstItemId = 1;
        public const int SecondItemId = 2;
        public const int NonExistentItemId = 999;
    }
}
