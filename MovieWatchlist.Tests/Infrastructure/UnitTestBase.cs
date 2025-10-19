using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Tests.Infrastructure;

/// <summary>
/// Base class for unit tests providing common setup and utilities
/// </summary>
public abstract class UnitTestBase
{

    /// <summary>
    /// Creates a test user with default values
    /// </summary>
    protected User CreateTestUser(int id = 1, string username = "testuser", string email = "test@example.com")
    {
        return TestDataBuilder.User()
            .WithId(id)
            .WithUsername(username)
            .WithEmail(email)
            .Build();
    }

    /// <summary>
    /// Creates a test movie with default values
    /// </summary>
    protected Movie CreateTestMovie(int id = 1, string title = "Test Movie", double voteAverage = 7.5)
    {
        return TestDataBuilder.Movie()
            .WithId(id)
            .WithTitle(title)
            .WithVoteAverage(voteAverage)
            .Build();
   }

    /// <summary>
    /// Creates a test watchlist item with default values
    /// </summary>
    protected WatchlistItem CreateTestWatchlistItem(int id = 1, int userId = 1, int movieId = 1, WatchlistStatus status = WatchlistStatus.Planned)
    {
        return TestDataBuilder.WatchlistItem()
            .WithId(id)
            .WithUserId(userId)
            .WithMovieId(movieId)
            .WithStatus(status)
            .Build();
    }
}
