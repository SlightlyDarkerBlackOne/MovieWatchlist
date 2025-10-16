using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.DTOs;

namespace MovieWatchlist.Core.Interfaces;

public interface IWatchlistService
{
    Task<IEnumerable<WatchlistItem>> GetUserWatchlistAsync(int userId);
    Task<IEnumerable<WatchlistItem>> GetWatchlistByStatusAsync(int userId, WatchlistStatus status);
    Task<IEnumerable<WatchlistItem>> GetFavoriteMoviesAsync(int userId);
    Task<WatchlistStatistics> GetUserStatisticsAsync(int userId);
    Task<IEnumerable<Movie>> GetRecommendedMoviesAsync(int userId, int limit = 10);
    Task<IEnumerable<WatchlistItem>> GetWatchlistByGenreAsync(int userId, string genre);
    Task<IEnumerable<WatchlistItem>> GetWatchlistByYearRangeAsync(int userId, int startYear, int endYear);
    Task<IEnumerable<WatchlistItem>> GetWatchlistByRatingRangeAsync(int userId, double minRating, double maxRating);
    Task<WatchlistItem> AddToWatchlistAsync(int userId, int movieId, WatchlistStatus status = WatchlistStatus.Planned, AddToWatchlistDto? addDto = null);
    Task<WatchlistItem?> UpdateWatchlistItemAsync(int userId, int watchlistItemId, UpdateWatchlistItemDto updateDto);
    Task<bool> RemoveFromWatchlistAsync(int userId, int watchlistItemId);
    Task<WatchlistItem?> GetWatchlistItemByIdAsync(int userId, int watchlistItemId);
}

public class WatchlistStatistics
{
    public int TotalMovies { get; set; }
    public int WatchedMovies { get; set; }
    public int PlannedMovies { get; set; }
    public int FavoriteMovies { get; set; }
    public double AverageUserRating { get; set; }
    public double AverageTmdbRating { get; set; }
    public string MostWatchedGenre { get; set; } = string.Empty;
    public int MoviesThisYear { get; set; }
    public Dictionary<string, int> GenreBreakdown { get; set; } = new();
    public Dictionary<int, int> YearlyBreakdown { get; set; } = new();
} 