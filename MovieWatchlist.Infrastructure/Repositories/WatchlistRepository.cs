using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Infrastructure.Data;

namespace MovieWatchlist.Infrastructure.Repositories;

/// <summary>
/// Repository for WatchlistItem entity with domain-specific operations.
/// Extends the generic repository with watchlist-specific queries.
/// </summary>
public class WatchlistRepository : EfRepository<WatchlistItem>, IWatchlistRepository
{
    public WatchlistRepository(MovieWatchlistDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets all watchlist items for a specific user asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <returns>A collection of watchlist items for the user</returns>
    public async Task<IEnumerable<WatchlistItem>> GetByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(w => w.Movie)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.AddedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets watchlist items for a user filtered by status asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="status">The status to filter by</param>
    /// <returns>A collection of watchlist items with the specified status</returns>
    public async Task<IEnumerable<WatchlistItem>> GetByUserIdAndStatusAsync(int userId, WatchlistStatus status)
    {
        return await _dbSet
            .Include(w => w.Movie)
            .Where(w => w.UserId == userId && w.Status == status)
            .OrderByDescending(w => w.AddedDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a specific watchlist item for a user asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="watchlistItemId">The ID of the watchlist item</param>
    /// <returns>The watchlist item if found, null otherwise</returns>
    public async Task<WatchlistItem?> GetByUserIdAndIdAsync(int userId, int watchlistItemId)
    {
        return await _dbSet
            .Include(w => w.Movie)
            .FirstOrDefaultAsync(w => w.UserId == userId && w.Id == watchlistItemId);
    }

    /// <summary>
    /// Checks if a movie is already in a user's watchlist.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="movieId">The ID of the movie</param>
    /// <returns>True if the movie is in the watchlist, false otherwise</returns>
    public async Task<bool> IsMovieInUserWatchlistAsync(int userId, int movieId)
    {
        return await _dbSet
            .AnyAsync(w => w.UserId == userId && w.MovieId == movieId);
    }

    /// <summary>
    /// Gets the count of watchlist items for a user by status.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="status">The status to count</param>
    /// <returns>The number of items with the specified status</returns>
    public async Task<int> GetCountByUserIdAndStatusAsync(int userId, WatchlistStatus status)
    {
        return await _dbSet
            .CountAsync(w => w.UserId == userId && w.Status == status);
    }

    /// <summary>
    /// Gets the user's favorite movies asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="limit">Maximum number of results to return (default: 50)</param>
    /// <returns>A collection of favorite movies for the user</returns>
    public async Task<IEnumerable<WatchlistItem>> GetFavoritesByUserIdAsync(int userId, int limit = 50)
    {
        return await _dbSet
            .Include(w => w.Movie)
            .Where(w => w.UserId == userId && w.IsFavorite)
            .OrderByDescending(w => w.AddedDate)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Gets recently added watchlist items for a user asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="limit">Maximum number of results to return (default: 10)</param>
    /// <returns>A collection of recently added watchlist items</returns>
    public async Task<IEnumerable<WatchlistItem>> GetRecentlyAddedByUserIdAsync(int userId, int limit = 10)
    {
        return await _dbSet
            .Include(w => w.Movie)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.AddedDate)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Gets recently watched movies for a user asynchronously.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="limit">Maximum number of results to return (default: 10)</param>
    /// <returns>A collection of recently watched movies</returns>
    public async Task<IEnumerable<WatchlistItem>> GetRecentlyWatchedByUserIdAsync(int userId, int limit = 10)
    {
        return await _dbSet
            .Include(w => w.Movie)
            .Where(w => w.UserId == userId && w.Status == WatchlistStatus.Watched && w.WatchedDate.HasValue)
            .OrderByDescending(w => w.WatchedDate)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Updates the status of a watchlist item and sets the watched date if applicable.
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="watchlistItemId">The ID of the watchlist item</param>
    /// <param name="status">The new status</param>
    /// <returns>True if the item was found and updated, false otherwise</returns>
    public async Task<bool> UpdateStatusAsync(int userId, int watchlistItemId, WatchlistStatus status)
    {
        var item = await _dbSet
            .FirstOrDefaultAsync(w => w.UserId == userId && w.Id == watchlistItemId);

        if (item == null)
            return false;

        // Use domain method instead of direct property assignment
        // This automatically handles WatchedDate when status becomes Watched
        item.UpdateStatus(status);

        _dbSet.Update(item);
        return true;
    }
}


