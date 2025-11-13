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
        
        public const string ExistingUsername = "existinguser";
        public const string ExistingEmail = "existing@example.com";
        public const string NonexistentUsername = "nonexistent";
        public const string NonexistentEmail = "nonexistent@example.com";
        public const string InvalidEmail = "invalid-email";
        public const string InvalidUsername = "user@name";
        
        // Invalid passwords for testing
        public const string WeakPassword = "weak";
        public const string ValidPassword1 = "Password1@";
        public const string ValidPassword2 = "Password2@";
        public const string WrongPassword = "WrongPassword123!";
        public const string CorrectPassword = "CorrectPassword123!";
        public const string NewPassword = "NewPassword123!";
        
        public const string TestUser1Username = "user1";
        public const string TestUser1Email = "user1@example.com";
        public const string TestUser2Username = "user2";
        public const string TestUser2Email = "user2@example.com";
        public const int IntegrationTestUserId = 123;
        public const string IntegrationTestUsername = "integrationtest";
        public const string IntegrationTestEmail = "integration@test.com";
        
        // Invalid password scenarios
        public const string NoUppercasePassword = "password";
        public const string NoLowercasePassword = "PASSWORD";
        public const string NoNumberPassword = "Password!";
        public const string NoSpecialCharPassword = "Password1";
        public const string TooShortPassword = "Pass1!";
        public const string NoSpecialCharLongPassword = "Password123";
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
        
        public const int TestTmdbId = 123;
        public const string TestReleaseDate = "2023-01-01";
        public const double TestVoteAverage = 8.5;
        public const double TestPopularity = 85.5;
        public const string TestOverview = "Test overview";
    }

    // Test JWT Data
    public static class Jwt
    {
        public const string TestSecretKey = "TestSecretKeyForJWTTokenGeneration123456789";
        public const string TestIssuer = "MovieWatchlistTestAPI";
        public const string TestAudience = "MovieWatchlistTestUsers";
        public const int TestExpirationMinutes = 60;
        public const int TestRefreshTokenExpirationDays = 7;
        public const string TestJwtToken = "test-jwt-token";
        public const string TestRefreshToken = "test-refresh-token";
        public const string NewJwtToken = "new-jwt-token";
        public const string NewRefreshToken = "new-refresh-token";
        public const string ValidRefreshToken = "valid-refresh-token";
        public const string ValidJwtToken = "valid-jwt-token";
        public const string InvalidJwtToken = "invalid-jwt-token";
        public const string InvalidRefreshToken = "invalid-refresh-token";
        public const string ExpiredRefreshToken = "expired-refresh-token";
        public const string RevokedRefreshToken = "revoked-refresh-token";
        public const string InvalidToken = "invalid-token";
        
        public const string WrongSecretKey = "WrongSecretKey123456789012345678901234567890";
        public const string WrongIssuer = "WrongIssuer";
        public const string WrongAudience = "WrongAudience";
        
        public const int ExpiredTokenMinutesAgo = 60;
        public const int ExpiredTokenValidMinutesAgo = 120;
        
        public const string Base64RegexPattern = "^[A-Za-z0-9+/]*={0,2}$";
        public const string UsernameClaimName = "username";
        public const string EmailClaimName = "email";
        public const string ExpirationClaimName = "exp";
    }

    // Test Cookie Names
    public static class CookieNames
    {
        public const string AccessToken = "accessToken";
        public const string RefreshToken = "refreshToken";
    }

    // Test HTTP Headers
    public static class HttpHeaders
    {
        public const string SetCookie = "Set-Cookie";
        public const string Cookie = "Cookie";
    }

    // Test Cookie Attributes
    public static class CookieAttributes
    {
        public const string HttpOnly = "HttpOnly";
        public const string SameSiteStrict = "SameSite=Strict";
        public const string SameSiteLax = "SameSite=Lax";
        public const string Expires = "expires=";
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
        public const string AuthMe = "/api/Auth/me";
        
        public const string MoviesSearch = "/api/Movies/search";
        public const string MoviesPopular = "/api/Movies/popular";
        public const string MoviesDetails = "/api/Movies/{id}";
        public const string MoviesGenre = "/api/Movies/genre/{genre}";
        
        public const string WatchlistMe = "/api/Watchlist/me/watchlist";
        public const string WatchlistMeAdd = "/api/Watchlist/me/watchlist/add";
        public const string WatchlistMeItem = "/api/Watchlist/me/watchlist/item";
        public const string WatchlistMeStatistics = "/api/Watchlist/me/watchlist/statistics";
        public const string WatchlistMeFavorites = "/api/Watchlist/me/watchlist/favorites";
        public const string WatchlistMeRecommendations = "/api/Watchlist/me/watchlist/recommendations";
        public const string WatchlistMeGenre = "/api/Watchlist/me/watchlist/genre/{genre}";
        public const string WatchlistMeYearRange = "/api/Watchlist/me/watchlist/year-range";
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
    
    // Test Display Values
    public static class DisplayValues
    {
        public const string MaskedPassword = "********";
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
