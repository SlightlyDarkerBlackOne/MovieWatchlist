using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.ValueObjects;
using MovieWatchlist.Persistence.Data;
using MovieWatchlist.Tests.Shared.TestDataBuilders;

namespace MovieWatchlist.Tests.Shared.Infrastructure;

/// <summary>
/// Utility class for seeding test databases with consistent test data
/// </summary>
public static class TestDatabaseSeeder
{
    /// <summary>
    /// Seeds the database with a complete set of test data
    /// </summary>
    public static async Task SeedTestDataAsync(MovieWatchlistDbContext context)
    {
        await SeedUsersAsync(context);
        await SeedMoviesAsync(context);
        await SeedWatchlistItemsAsync(context);
        await SeedRefreshTokensAsync(context);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds the database with basic test users
    /// </summary>
    public static async Task SeedUsersAsync(MovieWatchlistDbContext context)
    {
        var users = new[]
        {
            CreateUser(1, "testuser1", "testuser1@example.com", "hashed_password_1", DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(-1)),
            CreateUser(2, "testuser2", "testuser2@example.com", "hashed_password_2", DateTime.UtcNow.AddDays(-40), DateTime.UtcNow.AddDays(-3)),
            CreateUser(3, "inactiveuser", "inactive@example.com", "hashed_password_3", DateTime.UtcNow.AddDays(-90), null)
        };

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync(); // Save to get IDs
    }

    /// <summary>
    /// Seeds the database with test movies
    /// </summary>
    public static async Task SeedMoviesAsync(MovieWatchlistDbContext context)
    {
        var movies = new[]
        {
            TestDataBuilder.Movie()
                .WithTmdbId(12345)
                .WithTitle("The Dark Knight")
                .WithOverview("A crime thriller about Batman")
                .WithReleaseDate(new DateTime(2008, 7, 18, 0, 0, 0, DateTimeKind.Utc))
                .WithVoteAverage(9.0)
                .WithVoteCount(25000)
                .WithPopularity(85.5)
                .WithGenres("Action", "Crime", "Drama")
                .WithPosterPath("/qJ2tW6WMUDux911r6m7haRef0WH.jpg")
                .Build(),

            TestDataBuilder.Movie()
                .WithTmdbId(23456)
                .WithTitle("Inception")
                .WithOverview("A mind-bending thriller about dreams")
                .WithReleaseDate(new DateTime(2010, 7, 16, 0, 0, 0, DateTimeKind.Utc))
                .WithVoteAverage(8.8)
                .WithVoteCount(22000)
                .WithPopularity(78.2)
                .WithGenres("Action", "Sci-Fi", "Thriller")
                .WithPosterPath("/9gk7adHYeDvHkCSEqAvQNLV5Uge.jpg")
                .Build(),

            TestDataBuilder.Movie()
                .WithTmdbId(34567)
                .WithTitle("The Shawshank Redemption")
                .WithOverview("A story of hope and friendship in prison")
                .WithReleaseDate(new DateTime(1994, 9, 23, 0, 0, 0, DateTimeKind.Utc))
                .WithVoteAverage(9.3)
                .WithVoteCount(28000)
                .WithPopularity(92.1)
                .WithGenres("Drama")
                .WithPosterPath("/q6y0Go1tsGEsmtFryDOJo3dEmqu.jpg")
                .Build(),

            TestDataBuilder.Movie()
                .WithTmdbId(45678)
                .WithTitle("Pulp Fiction")
                .WithOverview("Interconnected stories of crime in Los Angeles")
                .WithReleaseDate(new DateTime(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc))
                .WithVoteAverage(8.9)
                .WithVoteCount(24000)
                .WithPopularity(88.7)
                .WithGenres("Crime", "Drama")
                .WithPosterPath("/d5iIlFn5s0ImszYzBPb8JPIfbXD.jpg")
                .Build(),

            TestDataBuilder.Movie()
                .WithTmdbId(56789)
                .WithTitle("The Matrix")
                .WithOverview("A computer hacker learns about the true nature of reality")
                .WithReleaseDate(new DateTime(1999, 3, 31, 0, 0, 0, DateTimeKind.Utc))
                .WithVoteAverage(8.7)
                .WithVoteCount(20000)
                .WithPopularity(75.3)
                .WithGenres("Action", "Sci-Fi")
                .WithPosterPath("/f89U3ADr1oiB1s9GkdPOEpXUk5H.jpg")
                .Build()
        };

        await context.Movies.AddRangeAsync(movies);
        await context.SaveChangesAsync(); // Save to get IDs
    }

    /// <summary>
    /// Seeds the database with test watchlist items
    /// </summary>
    public static async Task SeedWatchlistItemsAsync(MovieWatchlistDbContext context)
    {
        // Get the seeded users and movies to use their actual IDs
        var users = await context.Users.OrderBy(u => u.Id).ToListAsync();
        var movies = await context.Movies.OrderBy(m => m.Id).ToListAsync();

        var watchlistItems = new[]
        {
            CreateWatchlistItem(users[0].Id, movies[0], WatchlistStatus.Watched, true, 10, "Amazing movie!", DateTime.UtcNow.AddDays(-7), DateTime.UtcNow.AddDays(-3)),
            CreateWatchlistItem(users[0].Id, movies[1], WatchlistStatus.Watched, false, 8, "Confusing but good", DateTime.UtcNow.AddDays(-12), DateTime.UtcNow.AddDays(-4)),
            CreateWatchlistItem(users[0].Id, movies[2], WatchlistStatus.Planned, true, null, "Must watch soon", DateTime.UtcNow.AddDays(-10), null),
            CreateWatchlistItem(users[0].Id, movies[3], WatchlistStatus.Watching, false, null, "Currently watching", DateTime.UtcNow.AddDays(-8), null),
            CreateWatchlistItem(users[1].Id, movies[0], WatchlistStatus.Watched, true, 9, "Great superhero movie", DateTime.UtcNow.AddDays(-17), DateTime.UtcNow.AddDays(-8)),
            CreateWatchlistItem(users[1].Id, movies[4], WatchlistStatus.Planned, false, null, "Classic sci-fi", DateTime.UtcNow.AddDays(-14), null)
        };

        await context.WatchlistItems.AddRangeAsync(watchlistItems);
    }

    private static WatchlistItem CreateWatchlistItem(int userId, Movie movie, WatchlistStatus status, bool isFavorite, int? rating, string? notes, DateTime addedDate, DateTime? watchedDate)
    {
        var item = WatchlistItem.Create(userId, movie);
        
        if (status != WatchlistStatus.Planned)
            item.UpdateStatus(status);
            
        if (isFavorite)
            item.ToggleFavorite();
            
        if (rating.HasValue)
            item.SetRating(Rating.Create(rating.Value).Value!);
            
        if (!string.IsNullOrEmpty(notes))
            item.UpdateNotes(notes);
            
        // Note: AddedDate and WatchedDate are set by the domain methods, but we can't override them
        // The test data will use the current timestamps instead of the specified ones
        
        return item;
    }

    /// <summary>
    /// Seeds the database with test refresh tokens
    /// </summary>
    public static async Task SeedRefreshTokensAsync(MovieWatchlistDbContext context)
    {
        // Get the seeded users to use their actual IDs
        var users = await context.Users.OrderBy(u => u.Id).ToListAsync();

        var refreshTokens = new[]
        {
            CreateRefreshToken(users[0].Id, "valid_refresh_token_1", 7, false, DateTime.UtcNow.AddDays(-30)),
            CreateRefreshToken(users[1].Id, "valid_refresh_token_2", 7, false, DateTime.UtcNow.AddDays(-30)),
            TestDataBuilder.RefreshToken()
                .WithUserId(users[0].Id)
                .WithToken("expired_refresh_token")
                .WithExpiresAt(DateTime.UtcNow.AddDays(-30))
                .WithIsRevoked(false)
                .WithCreatedAt(DateTime.UtcNow.AddDays(-30))
                .Build(),
            CreateRefreshToken(users[1].Id, "revoked_refresh_token", 7, true, DateTime.UtcNow.AddDays(-30))
        };

        await context.RefreshTokens.AddRangeAsync(refreshTokens);
    }

    /// <summary>
    /// Clears all test data from the database
    /// </summary>
    public static async Task ClearTestDataAsync(MovieWatchlistDbContext context)
    {
        context.RefreshTokens.RemoveRange(context.RefreshTokens);
        context.WatchlistItems.RemoveRange(context.WatchlistItems);
        context.Movies.RemoveRange(context.Movies);
        context.Users.RemoveRange(context.Users);
        
        await context.SaveChangesAsync();
    }

    private static User CreateUser(int id, string username, string email, string passwordHash, DateTime createdAt, DateTime? lastLoginAt)
    {
        var user = User.Create(
            Username.Create(username).Value!,
            Email.Create(email).Value!,
            passwordHash
        );
        typeof(User).GetProperty("Id")!.SetValue(user, id);
        typeof(User).GetProperty("CreatedAt")!.SetValue(user, createdAt);
        if (lastLoginAt.HasValue)
        {
            typeof(User).GetProperty("LastLoginAt")!.SetValue(user, lastLoginAt.Value);
        }
        return user;
    }

    private static RefreshToken CreateRefreshToken(int userId, string token, int expirationDays, bool isRevoked, DateTime createdAt)
    {
        var refreshToken = RefreshToken.Create(userId, token, expirationDays);
        typeof(RefreshToken).GetProperty("IsRevoked")!.SetValue(refreshToken, isRevoked);
        typeof(RefreshToken).GetProperty("CreatedAt")!.SetValue(refreshToken, createdAt);
        return refreshToken;
    }
}
