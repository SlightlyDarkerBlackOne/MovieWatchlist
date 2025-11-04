using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Queries;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Core.Interfaces;

public interface IWatchlistService
{
    Task<IEnumerable<WatchlistItem>> GetUserWatchlistAsync(GetMyWatchlistQuery query);
    Task<IEnumerable<WatchlistItem>> GetWatchlistByStatusAsync(GetMyWatchlistByStatusQuery query);
    Task<IEnumerable<WatchlistItem>> GetFavoriteMoviesAsync(GetMyFavoriteMoviesQuery query);
    Task<WatchlistStatistics> GetUserStatisticsAsync(GetMyStatisticsQuery query);
    Task<IEnumerable<Movie>> GetRecommendedMoviesAsync(GetMyRecommendedMoviesQuery query);
    Task<IEnumerable<WatchlistItem>> GetWatchlistByGenreAsync(GetMyWatchlistByGenreQuery query);
    Task<IEnumerable<WatchlistItem>> GetWatchlistByYearRangeAsync(GetMyWatchlistByYearRangeQuery query);
    Task<IEnumerable<WatchlistItem>> GetWatchlistByRatingRangeAsync(GetMyWatchlistByRatingRangeQuery query);
    Task<WatchlistItem?> GetWatchlistItemByIdAsync(GetMyWatchlistItemByIdQuery query);
    
    Task<Result<WatchlistItem>> AddToWatchlistAsync(AddToWatchlistCommand command);
    Task<Result<WatchlistItem>> UpdateWatchlistItemAsync(UpdateWatchlistItemCommand command);
    Task<Result<bool>> RemoveFromWatchlistAsync(RemoveFromWatchlistCommand command);
} 