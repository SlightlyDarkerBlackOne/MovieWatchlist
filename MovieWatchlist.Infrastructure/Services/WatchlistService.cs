using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.DTOs;

namespace MovieWatchlist.Infrastructure.Services;

public class WatchlistService : IWatchlistService
{
    private readonly IWatchlistRepository _watchlistRepository;
    private readonly IRepository<Movie> _movieRepository;
    private readonly ITmdbService _tmdbService;
    private readonly IUnitOfWork _unitOfWork;

    public WatchlistService(
        IWatchlistRepository watchlistRepository,
        IRepository<Movie> movieRepository,
        ITmdbService tmdbService,
        IUnitOfWork unitOfWork)
    {
        _watchlistRepository = watchlistRepository;
        _movieRepository = movieRepository;
        _tmdbService = tmdbService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<WatchlistItem>> GetUserWatchlistAsync(int userId)
    {
        return await _watchlistRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<WatchlistItem>> GetWatchlistByStatusAsync(int userId, WatchlistStatus status)
    {
        return await _watchlistRepository.GetByUserIdAndStatusAsync(userId, status);
    }

    public async Task<IEnumerable<WatchlistItem>> GetFavoriteMoviesAsync(int userId)
    {
        return await _watchlistRepository.GetFavoritesByUserIdAsync(userId);
    }

    public async Task<WatchlistStatistics> GetUserStatisticsAsync(int userId)
    {
        var watchlist = await _watchlistRepository.GetByUserIdAsync(userId);
        var watchlistList = watchlist.ToList();

        var statistics = new WatchlistStatistics
        {
            TotalMovies = watchlistList.Count,
            WatchedMovies = watchlistList.Count(w => w.Status == WatchlistStatus.Watched),
            PlannedMovies = watchlistList.Count(w => w.Status == WatchlistStatus.Planned),
            FavoriteMovies = watchlistList.Count(w => w.IsFavorite),
            MoviesThisYear = watchlistList.Count(w => w.AddedDate.Year == DateTime.UtcNow.Year)
        };

        // Calculate average ratings using LINQ
        var ratedMovies = watchlistList.Where(w => w.UserRating.HasValue).ToList();
        if (ratedMovies.Any())
        {
            statistics.AverageUserRating = ratedMovies.Average(w => w.UserRating!.Value);
        }

        if (watchlistList.Any())
        {
            statistics.AverageTmdbRating = watchlistList.Average(w => w.Movie.VoteAverage);
        }

        // Genre breakdown using LINQ GroupBy
        var genreGroups = watchlistList
            .Where(w => w.Status == WatchlistStatus.Watched)
            .SelectMany(w => w.Movie.Genres)
            .GroupBy(g => g)
            .OrderByDescending(g => g.Count());

        statistics.GenreBreakdown = genreGroups.ToDictionary(g => g.Key, g => g.Count());
        statistics.MostWatchedGenre = genreGroups.FirstOrDefault()?.Key ?? string.Empty;

        // Yearly breakdown using LINQ GroupBy
        var yearlyGroups = watchlistList
            .Where(w => w.Status == WatchlistStatus.Watched)
            .GroupBy(w => w.Movie.ReleaseDate.Year)
            .OrderByDescending(g => g.Key);

        statistics.YearlyBreakdown = yearlyGroups.ToDictionary(g => g.Key, g => g.Count());

        return statistics;
    }

    public async Task<IEnumerable<Movie>> GetRecommendedMoviesAsync(int userId, int limit = 10)
    {
        var userWatchlist = await _watchlistRepository.GetByUserIdAsync(userId);
        var userWatchlistList = userWatchlist.ToList();

        // Get user's favorite genres based on watched movies
        var favoriteGenres = userWatchlistList
            .Where(w => w.Status == WatchlistStatus.Watched && w.UserRating >= 4)
            .SelectMany(w => w.Movie.Genres)
            .GroupBy(g => g)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToList();

        // Get all movies and filter by favorite genres
        var allMovies = await _movieRepository.GetAllAsync();
        
        var recommendedMovies = allMovies
            .Where(m => !userWatchlistList.Any(w => w.MovieId == m.Id)) // Not in user's watchlist
            .Where(m => m.Genres.Any(g => favoriteGenres.Contains(g))) // Matches favorite genres
            .Where(m => m.VoteAverage >= 7.0) // High TMDB rating
            .Where(m => m.VoteCount >= 1000) // Sufficient votes
            .OrderByDescending(m => m.Popularity)
            .ThenByDescending(m => m.VoteAverage)
            .Take(limit);

        return recommendedMovies;
    }

    public async Task<IEnumerable<WatchlistItem>> GetWatchlistByGenreAsync(int userId, string genre)
    {
        var watchlist = await _watchlistRepository.GetByUserIdAsync(userId);
        return watchlist
            .Where(w => w.Movie.Genres.Contains(genre))
            .OrderByDescending(w => w.AddedDate);
    }

    public async Task<IEnumerable<WatchlistItem>> GetWatchlistByYearRangeAsync(int userId, int startYear, int endYear)
    {
        var watchlist = await _watchlistRepository.GetByUserIdAsync(userId);
        return watchlist
            .Where(w => w.Movie.ReleaseDate.Year >= startYear && w.Movie.ReleaseDate.Year <= endYear)
            .OrderByDescending(w => w.Movie.ReleaseDate);
    }

    public async Task<IEnumerable<WatchlistItem>> GetWatchlistByRatingRangeAsync(int userId, double minRating, double maxRating)
    {
        var watchlist = await _watchlistRepository.GetByUserIdAsync(userId);
        return watchlist
            .Where(w => w.Movie.VoteAverage >= minRating && w.Movie.VoteAverage <= maxRating)
            .OrderByDescending(w => w.Movie.VoteAverage);
    }


    public async Task<WatchlistItem> AddToWatchlistAsync(int userId, int movieId, WatchlistStatus status = WatchlistStatus.Planned, AddToWatchlistDto? addDto = null)
    {
        // movieId parameter is the TMDB ID - check if we already have this movie cached in our database
        var existingMovies = await _movieRepository.FindAsync(m => m.TmdbId == movieId);
        var cachedMovie = existingMovies.FirstOrDefault();
        
        Movie movie;
        if (cachedMovie == null)
        {
            // Not in cache - fetch from TMDB and save to database
            var tmdbMovie = await _tmdbService.GetMovieDetailsAsync(movieId);
            if (tmdbMovie == null)
                throw new ArgumentException($"Movie with TMDB ID {movieId} not found");
            
            // Save to database as cache and persist immediately to get the database ID
            await _movieRepository.AddAsync(tmdbMovie);
            await _unitOfWork.SaveChangesAsync();
            movie = tmdbMovie;
        }
        else
        {
            // Already cached - use it
            movie = cachedMovie;
        }

        // Check if already in user's watchlist
        var existingItem = await _watchlistRepository.FindAsync(w => w.UserId == userId && w.MovieId == movie.Id);
        if (existingItem.Any())
            throw new InvalidOperationException($"Movie is already in user's watchlist");

        var watchlistItem = new WatchlistItem
        {
            UserId = userId,
            MovieId = movie.Id, // Store our database ID (not TMDB ID)
            Movie = movie, // Set navigation property to ensure it's available in response
            Status = status,
            AddedDate = DateTime.UtcNow,
            WatchedDate = status == WatchlistStatus.Watched ? DateTime.UtcNow : null,
            Notes = addDto?.Notes
        };

        await _watchlistRepository.AddAsync(watchlistItem);
        await _unitOfWork.SaveChangesAsync();
        return watchlistItem;
    }

    public async Task<WatchlistItem?> UpdateWatchlistItemAsync(int userId, int watchlistItemId, UpdateWatchlistItemDto updateDto)
    {
        var item = await _watchlistRepository.GetByUserIdAndIdAsync(userId, watchlistItemId);
        
        if (item == null)
            return null;

        // Update properties if provided
        if (updateDto.Status.HasValue)
        {
            item.Status = updateDto.Status.Value;
            if (updateDto.Status.Value == WatchlistStatus.Watched && !item.WatchedDate.HasValue)
                item.WatchedDate = DateTime.UtcNow;
        }

        if (updateDto.IsFavorite.HasValue)
            item.IsFavorite = updateDto.IsFavorite.Value;

        if (updateDto.UserRating.HasValue)
            item.UserRating = updateDto.UserRating.Value;

        if (updateDto.Notes != null)
            item.Notes = updateDto.Notes;

        if (updateDto.WatchedDate.HasValue)
            item.WatchedDate = updateDto.WatchedDate.Value;

        await _watchlistRepository.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return item;
    }

    public async Task<bool> RemoveFromWatchlistAsync(int userId, int watchlistItemId)
    {
        var watchlistItem = await _watchlistRepository.FindAsync(w => w.Id == watchlistItemId && w.UserId == userId);
        var item = watchlistItem.FirstOrDefault();
        
        if (item == null)
            return false;

        await _watchlistRepository.DeleteAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<WatchlistItem?> GetWatchlistItemByIdAsync(int userId, int watchlistItemId)
    {
        return await _watchlistRepository.GetByUserIdAndIdAsync(userId, watchlistItemId);
    }
} 