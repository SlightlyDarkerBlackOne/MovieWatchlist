using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.ValueObjects;
using MovieWatchlist.Infrastructure.Data;

namespace MovieWatchlist.Tests.Infrastructure;

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
            TestDataBuilder.User()
                .WithId(1)
                .WithUsername("testuser1")
                .WithEmail("testuser1@example.com")
                .WithPasswordHash("hashed_password_1")
                .WithCreatedAt(DateTime.UtcNow.AddDays(-30))
                .WithLastLoginAt(DateTime.UtcNow.AddDays(-1))
                .Build(),

            TestDataBuilder.User()
                .WithId(2)
                .WithUsername("testuser2")
                .WithEmail("testuser2@example.com")
                .WithPasswordHash("hashed_password_2")
                .WithCreatedAt(DateTime.UtcNow.AddDays(-40))
                .WithLastLoginAt(DateTime.UtcNow.AddDays(-3))
                .Build(),

            TestDataBuilder.User()
                .WithId(3)
                .WithUsername("inactiveuser")
                .WithEmail("inactive@example.com")
                .WithPasswordHash("hashed_password_3")
                .WithCreatedAt(DateTime.UtcNow.AddDays(-90))
                .WithLastLoginAt(null)
                .Build()
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
            // User 1's watchlist
            TestDataBuilder.WatchlistItem()
                .WithUserId(users[0].Id)
                .WithMovieId(movies[0].Id)
                .WithStatus(WatchlistStatus.Watched)
                .WithIsFavorite(true)
                .WithUserRating(Rating.Create(10).Value!)
                .WithNotes("Amazing movie!")
                .WithAddedDate(DateTime.UtcNow.AddDays(-7))
                .WithWatchedDate(DateTime.UtcNow.AddDays(-3))
                .Build(),

            TestDataBuilder.WatchlistItem()
                .WithUserId(users[0].Id)
                .WithMovieId(movies[1].Id)
                .WithStatus(WatchlistStatus.Watched)
                .WithIsFavorite(false)
                .WithUserRating(Rating.Create(8).Value!)
                .WithNotes("Confusing but good")
                .WithAddedDate(DateTime.UtcNow.AddDays(-12))
                .WithWatchedDate(DateTime.UtcNow.AddDays(-4))
                .Build(),

            TestDataBuilder.WatchlistItem()
                .WithUserId(users[0].Id)
                .WithMovieId(movies[2].Id)
                .WithStatus(WatchlistStatus.Planned)
                .WithIsFavorite(true)
                .WithUserRating(null)
                .WithNotes("Must watch soon")
                .WithAddedDate(DateTime.UtcNow.AddDays(-10))
                .WithWatchedDate(null)
                .Build(),

            TestDataBuilder.WatchlistItem()
                .WithUserId(users[0].Id)
                .WithMovieId(movies[3].Id)
                .WithStatus(WatchlistStatus.Watching)
                .WithIsFavorite(false)
                .WithUserRating(null)
                .WithNotes("Currently watching")
                .WithAddedDate(DateTime.UtcNow.AddDays(-8))
                .WithWatchedDate(null)
                .Build(),

            // User 2's watchlist
            TestDataBuilder.WatchlistItem()
                .WithUserId(users[1].Id)
                .WithMovieId(movies[0].Id)
                .WithStatus(WatchlistStatus.Watched)
                .WithIsFavorite(true)
                .WithUserRating(Rating.Create(9).Value!)
                .WithNotes("Great superhero movie")
                .WithAddedDate(DateTime.UtcNow.AddDays(-17))
                .WithWatchedDate(DateTime.UtcNow.AddDays(-8))
                .Build(),

            TestDataBuilder.WatchlistItem()
                .WithUserId(users[1].Id)
                .WithMovieId(movies[4].Id)
                .WithStatus(WatchlistStatus.Planned)
                .WithIsFavorite(false)
                .WithUserRating(null)
                .WithNotes("Classic sci-fi")
                .WithAddedDate(DateTime.UtcNow.AddDays(-14))
                .WithWatchedDate(null)
                .Build()
        };

        await context.WatchlistItems.AddRangeAsync(watchlistItems);
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
            TestDataBuilder.RefreshToken()
                .WithId(1)
                .WithUserId(users[0].Id)
                .WithToken("valid_refresh_token_1")
                .WithExpiresAt(DateTime.UtcNow.AddDays(7))
                .WithIsRevoked(false)
                .WithCreatedAt(DateTime.UtcNow.AddDays(-30))
                .Build(),

            TestDataBuilder.RefreshToken()
                .WithId(2)
                .WithUserId(users[1].Id)
                .WithToken("valid_refresh_token_2")
                .WithExpiresAt(DateTime.UtcNow.AddDays(7))
                .WithIsRevoked(false)
                .WithCreatedAt(DateTime.UtcNow.AddDays(-30))
                .Build(),

            TestDataBuilder.RefreshToken()
                .WithId(3)
                .WithUserId(users[0].Id)
                .WithToken("expired_refresh_token")
                .WithExpiresAt(DateTime.UtcNow.AddDays(-30))
                .WithIsRevoked(false)
                .WithCreatedAt(DateTime.UtcNow.AddDays(-30))
                .Build(),

            TestDataBuilder.RefreshToken()
                .WithId(4)
                .WithUserId(users[1].Id)
                .WithToken("revoked_refresh_token")
                .WithExpiresAt(DateTime.UtcNow.AddDays(7))
                .WithIsRevoked(true)
                .WithCreatedAt(DateTime.UtcNow.AddDays(-30))
                .Build()
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
}
