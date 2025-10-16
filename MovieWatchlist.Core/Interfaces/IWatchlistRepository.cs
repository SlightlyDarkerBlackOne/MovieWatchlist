using System.Collections.Generic;
using System.Threading.Tasks;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Core.Interfaces;

/// <summary>
/// Repository interface for WatchlistItem entity with domain-specific operations.
/// Extends the generic repository with watchlist-specific queries.
/// </summary>
public interface IWatchlistRepository : IRepository<WatchlistItem>
{
    /// <summary>
    /// Gets all watchlist items for a specific user asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>A collection of watchlist items for the user</returns>
    Task<IEnumerable<WatchlistItem>> GetByUserIdAsync(int userId);

    /// <summary>
    /// Gets watchlist items for a user filtered by status asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="status">The status to filter by</param>
    /// <returns>A collection of watchlist items with the specified status</returns>
    Task<IEnumerable<WatchlistItem>> GetByUserIdAndStatusAsync(int userId, WatchlistStatus status);

    /// <summary>
    /// Gets a specific watchlist item for a user asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="watchlistItemId">The ID of the watchlist item</param>
    /// <returns>The watchlist item if found, null otherwise</returns>
    Task<WatchlistItem?> GetByUserIdAndIdAsync(int userId, int watchlistItemId);

    /// <summary>
    /// Checks if a movie is already in a user's watchlist.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="movieId">The ID of the movie</param>
    /// <returns>True if the movie is in the watchlist, false otherwise</returns>
    Task<bool> IsMovieInUserWatchlistAsync(int userId, int movieId);

    /// <summary>
    /// Gets the count of watchlist items for a user by status.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="status">The status to count</param>
    /// <returns>The number of items with the specified status</returns>
    Task<int> GetCountByUserIdAndStatusAsync(int userId, WatchlistStatus status);

    /// <summary>
    /// Gets the user's favorite movies asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="limit">Maximum number of results to return (default: 50)</param>
    /// <returns>A collection of favorite movies for the user</returns>
    Task<IEnumerable<WatchlistItem>> GetFavoritesByUserIdAsync(int userId, int limit = 50);

    /// <summary>
    /// Gets recently added watchlist items for a user asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="limit">Maximum number of results to return (default: 10)</param>
    /// <returns>A collection of recently added watchlist items</returns>
    Task<IEnumerable<WatchlistItem>> GetRecentlyAddedByUserIdAsync(int userId, int limit = 10);

    /// <summary>
    /// Gets recently watched movies for a user asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="limit">Maximum number of results to return (default: 10)</param>
    /// <returns>A collection of recently watched movies</returns>
    Task<IEnumerable<WatchlistItem>> GetRecentlyWatchedByUserIdAsync(int userId, int limit = 10);

    /// <summary>
    /// Updates the status of a watchlist item and sets the watched date if applicable.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="watchlistItemId">The ID of the watchlist item</param>
    /// <param name="status">The new status</param>
    /// <returns>True if the item was found and updated, false otherwise</returns>
    Task<bool> UpdateStatusAsync(int userId, int watchlistItemId, WatchlistStatus status);
}


