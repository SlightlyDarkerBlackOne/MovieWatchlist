using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Queries;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Core.Interfaces;

public interface IWatchlistService
{
    // Queries
    Task<IEnumerable<WatchlistItem>> GetUserWatchlistAsync(GetUserWatchlistQuery query);
    Task<IEnumerable<WatchlistItem>> GetWatchlistByStatusAsync(GetWatchlistByStatusQuery query);
    Task<IEnumerable<WatchlistItem>> GetFavoriteMoviesAsync(GetFavoriteMoviesQuery query);
    Task<WatchlistStatistics> GetUserStatisticsAsync(GetUserStatisticsQuery query);
    Task<IEnumerable<Movie>> GetRecommendedMoviesAsync(GetRecommendedMoviesQuery query);
    Task<IEnumerable<WatchlistItem>> GetWatchlistByGenreAsync(GetWatchlistByGenreQuery query);
    Task<IEnumerable<WatchlistItem>> GetWatchlistByYearRangeAsync(GetWatchlistByYearRangeQuery query);
    Task<IEnumerable<WatchlistItem>> GetWatchlistByRatingRangeAsync(GetWatchlistByRatingRangeQuery query);
    Task<WatchlistItem?> GetWatchlistItemByIdAsync(GetWatchlistItemByIdQuery query);
    
    // Commands
    Task<WatchlistItem> AddToWatchlistAsync(AddToWatchlistCommand command);
    Task<WatchlistItem?> UpdateWatchlistItemAsync(UpdateWatchlistItemCommand command);
    Task<bool> RemoveFromWatchlistAsync(RemoveFromWatchlistCommand command);
} 