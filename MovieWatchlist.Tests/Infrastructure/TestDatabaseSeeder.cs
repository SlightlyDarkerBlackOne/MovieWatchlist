using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Core.Models;
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
        if (await context.Users.AnyAsync())
            return;

        var users = new[]
        {
            TestDataBuilder.User()
                .WithId(1)
                .WithUsername("testuser1")
                .WithEmail("testuser1@example.com")
                .WithPasswordHash("hashed_password_1")
                .WithCreatedAt(TestConstants.Dates.DefaultCreatedAt)
                .WithLastLoginAt(TestConstants.Dates.DefaultLastLoginAt)
                .Build(),

            TestDataBuilder.User()
                .WithId(2)
                .WithUsername("testuser2")
                .WithEmail("testuser2@example.com")
                .WithPasswordHash("hashed_password_2")
                .WithCreatedAt(TestConstants.Dates.DefaultCreatedAt.AddDays(-10))
                .WithLastLoginAt(TestConstants.Dates.DefaultLastLoginAt.AddDays(-2))
                .Build(),

            TestDataBuilder.User()
                .WithId(3)
                .WithUsername("inactiveuser")
                .WithEmail("inactive@example.com")
                .WithPasswordHash("hashed_password_3")
                .WithCreatedAt(TestConstants.Dates.DefaultCreatedAt.AddDays(-60))
                .WithLastLoginAt(null)
                .Build()
        };

        await context.Users.AddRangeAsync(users);
    }

    /// <summary>
    /// Seeds the database with test movies
    /// </summary>
    public static async Task SeedMoviesAsync(MovieWatchlistDbContext context)
    {
        if (await context.Movies.AnyAsync())
            return;

        var movies = new[]
        {
            TestDataBuilder.Movie()
                .WithId(1)
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
                .WithId(2)
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
                .WithId(3)
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
                .WithId(4)
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
                .WithId(5)
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
    }

    /// <summary>
    /// Seeds the database with test watchlist items
    /// </summary>
    public static async Task SeedWatchlistItemsAsync(MovieWatchlistDbContext context)
    {
        if (await context.WatchlistItems.AnyAsync())
            return;

        var watchlistItems = new[]
        {
            // User 1's watchlist
            TestDataBuilder.WatchlistItem()
                .WithId(1)
                .WithUserId(1)
                .WithMovieId(1)
                .WithStatus(WatchlistStatus.Watched)
                .WithIsFavorite(true)
                .WithUserRating(10)
                .WithNotes("Amazing movie!")
                .WithAddedDate(TestConstants.Dates.DefaultAddedDate)
                .WithWatchedDate(TestConstants.Dates.DefaultWatchedDate)
                .Build(),

            TestDataBuilder.WatchlistItem()
                .WithId(2)
                .WithUserId(1)
                .WithMovieId(2)
                .WithStatus(WatchlistStatus.Watched)
                .WithIsFavorite(false)
                .WithUserRating(8)
                .WithNotes("Confusing but good")
                .WithAddedDate(TestConstants.Dates.DefaultAddedDate.AddDays(-5))
                .WithWatchedDate(TestConstants.Dates.DefaultWatchedDate.AddDays(-1))
                .Build(),

            TestDataBuilder.WatchlistItem()
                .WithId(3)
                .WithUserId(1)
                .WithMovieId(3)
                .WithStatus(WatchlistStatus.Planned)
                .WithIsFavorite(true)
                .WithUserRating(null)
                .WithNotes("Must watch soon")
                .WithAddedDate(TestConstants.Dates.DefaultAddedDate.AddDays(-3))
                .WithWatchedDate(null)
                .Build(),

            TestDataBuilder.WatchlistItem()
                .WithId(4)
                .WithUserId(1)
                .WithMovieId(4)
                .WithStatus(WatchlistStatus.Watching)
                .WithIsFavorite(false)
                .WithUserRating(null)
                .WithNotes("Currently watching")
                .WithAddedDate(TestConstants.Dates.DefaultAddedDate.AddDays(-1))
                .WithWatchedDate(null)
                .Build(),

            // User 2's watchlist
            TestDataBuilder.WatchlistItem()
                .WithId(5)
                .WithUserId(2)
                .WithMovieId(1)
                .WithStatus(WatchlistStatus.Watched)
                .WithIsFavorite(true)
                .WithUserRating(9)
                .WithNotes("Great superhero movie")
                .WithAddedDate(TestConstants.Dates.DefaultAddedDate.AddDays(-10))
                .WithWatchedDate(TestConstants.Dates.DefaultWatchedDate.AddDays(-5))
                .Build(),

            TestDataBuilder.WatchlistItem()
                .WithId(6)
                .WithUserId(2)
                .WithMovieId(5)
                .WithStatus(WatchlistStatus.Planned)
                .WithIsFavorite(false)
                .WithUserRating(null)
                .WithNotes("Classic sci-fi")
                .WithAddedDate(TestConstants.Dates.DefaultAddedDate.AddDays(-7))
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
        if (await context.RefreshTokens.AnyAsync())
            return;

        var refreshTokens = new[]
        {
            TestDataBuilder.RefreshToken()
                .WithId(1)
                .WithUserId(1)
                .WithToken("valid_refresh_token_1")
                .WithExpiresAt(TestConstants.Dates.DefaultTokenExpiresAt)
                .WithIsRevoked(false)
                .WithCreatedAt(TestConstants.Dates.DefaultCreatedAt)
                .Build(),

            TestDataBuilder.RefreshToken()
                .WithId(2)
                .WithUserId(2)
                .WithToken("valid_refresh_token_2")
                .WithExpiresAt(TestConstants.Dates.DefaultTokenExpiresAt)
                .WithIsRevoked(false)
                .WithCreatedAt(TestConstants.Dates.DefaultCreatedAt)
                .Build(),

            TestDataBuilder.RefreshToken()
                .WithId(3)
                .WithUserId(1)
                .WithToken("expired_refresh_token")
                .WithExpiresAt(DateTime.UtcNow.AddDays(-1))
                .WithIsRevoked(false)
                .WithCreatedAt(TestConstants.Dates.DefaultCreatedAt.AddDays(-10))
                .Build(),

            TestDataBuilder.RefreshToken()
                .WithId(4)
                .WithUserId(2)
                .WithToken("revoked_refresh_token")
                .WithExpiresAt(TestConstants.Dates.DefaultTokenExpiresAt)
                .WithIsRevoked(true)
                .WithCreatedAt(TestConstants.Dates.DefaultCreatedAt.AddDays(-5))
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
