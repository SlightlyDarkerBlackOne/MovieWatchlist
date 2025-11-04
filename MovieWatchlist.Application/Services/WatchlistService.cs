using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Exceptions;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Queries;
using MovieWatchlist.Core.Specifications;
using MovieWatchlist.Core.ValueObjects;

namespace MovieWatchlist.Application.Services;

public class WatchlistService : IWatchlistService
{
    private readonly IWatchlistRepository _watchlistRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITmdbService _tmdbService;
    private readonly IRetryPolicyService _retryPolicy;
    private readonly ICurrentUserService _currentUserService;

    public WatchlistService(
        IWatchlistRepository watchlistRepository,
        IMovieRepository movieRepository,
        IUserRepository userRepository,
        ITmdbService tmdbService,
        IRetryPolicyService retryPolicy,
        ICurrentUserService currentUserService)
    {
        _watchlistRepository = watchlistRepository;
        _movieRepository = movieRepository;
        _userRepository = userRepository;
        _tmdbService = tmdbService;
        _retryPolicy = retryPolicy;
        _currentUserService = currentUserService;
    }

    private int GetCurrentUserId()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User ID not found in authentication context");
        }
        return userId.Value;
    }

    public async Task<IEnumerable<WatchlistItem>> GetUserWatchlistAsync(GetMyWatchlistQuery query)
    {
        var userId = GetCurrentUserId();
        return await _watchlistRepository.GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<WatchlistItem>> GetWatchlistByStatusAsync(GetMyWatchlistByStatusQuery query)
    {
        var userId = GetCurrentUserId();
        return await _watchlistRepository.GetByUserIdAndStatusAsync(userId, query.Status);
    }

    public async Task<IEnumerable<WatchlistItem>> GetFavoriteMoviesAsync(GetMyFavoriteMoviesQuery query)
    {
        var userId = GetCurrentUserId();
        return await _watchlistRepository.GetFavoritesByUserIdAsync(userId);
    }

    public async Task<WatchlistStatistics> GetUserStatisticsAsync(GetMyStatisticsQuery query)
    {
        var userId = GetCurrentUserId();
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new ApiException("User not found");
        }

        // Return cached statistics if available
        var cachedStats = user.GetCachedStatistics();
        if (cachedStats != null)
            return cachedStats;

        // Calculate and cache new statistics
        var watchlist = await _watchlistRepository.GetByUserIdAsync(userId);
        var statistics = user.CalculateStatistics(watchlist);
        
        user.UpdateCachedStatistics(statistics);
        await _userRepository.UpdateAsync(user);
        
        return statistics;
    }

    public async Task<IEnumerable<Movie>> GetRecommendedMoviesAsync(GetMyRecommendedMoviesQuery query)
    {
        var userId = GetCurrentUserId();
        var userWatchlist = await _watchlistRepository.GetByUserIdAsync(userId);
        var userWatchlistList = userWatchlist.ToList();

        var watchedSpec = new WatchlistByStatusSpecification(WatchlistStatus.Watched);
        var highRatedSpec = new HighRatedMoviesSpecification();
        var combinedSpec = watchedSpec.And(highRatedSpec);

        var favoriteGenres = userWatchlistList
            .Where(combinedSpec.IsSatisfiedBy)
            .SelectMany(w => w.Movie.Genres)
            .GroupBy(g => g)
            .OrderByDescending(g => g.Count())
            .Take(ValidationConstants.Recommendation.TopGenresCount)
            .Select(g => g.Key)
            .ToList();

        var allMovies = await _movieRepository.GetAllAsync();
        
        var userMovieIds = userWatchlistList.Select(w => w.MovieId).ToArray();
        var notInWatchlistSpec = new MoviesNotInWatchlistSpecification(userMovieIds);
        var highRatedMovieSpec = new HighTmdbRatedMoviesSpecification(minRating: ValidationConstants.Recommendation.MinTmdbRating);
        var popularMovieSpec = new PopularMoviesSpecification(minVoteCount: ValidationConstants.Recommendation.MinVoteCount);
        
        var recommendedMovies = allMovies
            .Where(notInWatchlistSpec.IsSatisfiedBy)
            .Where(m => m.Genres.Any(g => favoriteGenres.Contains(g)))
            .Where(highRatedMovieSpec.IsSatisfiedBy)
            .Where(popularMovieSpec.IsSatisfiedBy)
            .OrderByDescending(m => m.Popularity)
            .ThenByDescending(m => m.VoteAverage)
            .Take(query.Limit);

        return recommendedMovies;
    }

    public async Task<IEnumerable<WatchlistItem>> GetWatchlistByGenreAsync(GetMyWatchlistByGenreQuery query)
    {
        var userId = GetCurrentUserId();
        var watchlist = await _watchlistRepository.GetByUserIdAsync(userId);
        var spec = new WatchlistByGenreSpecification(query.Genre);
        return watchlist
            .Where(spec.IsSatisfiedBy)
            .OrderByDescending(w => w.AddedDate);
    }

    public async Task<IEnumerable<WatchlistItem>> GetWatchlistByYearRangeAsync(GetMyWatchlistByYearRangeQuery query)
    {
        var userId = GetCurrentUserId();
        var watchlist = await _watchlistRepository.GetByUserIdAsync(userId);
        var spec = new WatchlistByYearRangeSpecification(query.StartYear, query.EndYear);
        return watchlist
            .Where(spec.IsSatisfiedBy)
            .OrderByDescending(w => w.Movie.ReleaseDate);
    }

    public async Task<IEnumerable<WatchlistItem>> GetWatchlistByRatingRangeAsync(GetMyWatchlistByRatingRangeQuery query)
    {
        var userId = GetCurrentUserId();
        var watchlist = await _watchlistRepository.GetByUserIdAsync(userId);
        var spec = new WatchlistByTmdbRatingRangeSpecification(query.MinRating, query.MaxRating);
        return watchlist
            .Where(spec.IsSatisfiedBy)
            .OrderByDescending(w => w.Movie.VoteAverage);
    }


    /// <summary>
    /// Adds a movie to the user's watchlist. The movieId parameter is the TMDB ID.
    /// If the movie is not cached locally, it fetches from TMDB and caches it.
    /// Creates a watchlist item with the specified status and optional notes.
    /// </summary>
    public async Task<Result<WatchlistItem>> AddToWatchlistAsync(AddToWatchlistCommand command)
    {
        var userId = GetCurrentUserId();
        var cachedMovie = await _movieRepository.GetByTmdbIdAsync(command.MovieId);
        
        Movie movie;
        if (cachedMovie == null)
        {
            var tmdbMovie = await _retryPolicy.ExecuteWithRetryAsync(
                () => _tmdbService.GetMovieDetailsAsync(command.MovieId),
                maxRetries: 3,
                baseDelayMs: 1000,
                operationName: "TMDB GetMovieDetails"
            );
            
            if (tmdbMovie == null)
                return Result<WatchlistItem>.Failure(string.Format(ErrorMessages.MovieNotFound, command.MovieId));
            
            await _movieRepository.AddAsync(tmdbMovie);
            movie = tmdbMovie;
        }
        else
        {
            movie = cachedMovie;
        }

        // Check if movie is already in watchlist using TMDB ID
        var existingItem = await _watchlistRepository.GetByUserIdAndTmdbIdAsync(userId, command.MovieId);
        if (existingItem != null)
        {
            return Result<WatchlistItem>.Failure(ErrorMessages.MovieAlreadyInWatchlist);
        }

        // For new movies, add them to the repository
        if (cachedMovie == null)
        {
            await _movieRepository.AddAsync(movie);
        }

        // Create watchlist item using the movie navigation property
        // EF Core will automatically set MovieId when SaveChanges is called
        var watchlistItem = WatchlistItem.Create(userId, movie);
        
        if (command.Status != WatchlistStatus.Planned)
        {
            watchlistItem.UpdateStatus(command.Status);
        }
        
        if (!string.IsNullOrEmpty(command.Notes))
        {
            watchlistItem.UpdateNotes(command.Notes);
        }

        await _watchlistRepository.AddAsync(watchlistItem);
        
        await InvalidateUserStatisticsAsync(userId);
        
        return Result<WatchlistItem>.Success(watchlistItem);
    }

    public async Task<Result<WatchlistItem>> UpdateWatchlistItemAsync(UpdateWatchlistItemCommand command)
    {
        var userId = GetCurrentUserId();
        var item = await _watchlistRepository.GetByUserIdAndIdAsync(userId, command.WatchlistItemId);
        
        if (item == null)
        {
            return Result<WatchlistItem>.Failure(ErrorMessages.WatchlistItemNotFound);
        }

        if (command.Status.HasValue)
        {
            item.UpdateStatus(command.Status.Value);
        }

        if (command.IsFavorite.HasValue)
        {
            if (item.IsFavorite != command.IsFavorite.Value)
            {
                item.ToggleFavorite();
            }
        }

        if (command.UserRating.HasValue)
        {
            var ratingResult = Rating.Create(command.UserRating.Value);
            if (ratingResult.IsFailure)
                return Result<WatchlistItem>.Failure(ratingResult.Error);
            
            item.SetRating(ratingResult.Value!);
        }

        if (command.Notes != null)
        {
            item.UpdateNotes(command.Notes);
        }

        await _watchlistRepository.UpdateAsync(item);
        
        await InvalidateUserStatisticsAsync(userId);
        
        return Result<WatchlistItem>.Success(item);
    }

    public async Task<Result<bool>> RemoveFromWatchlistAsync(RemoveFromWatchlistCommand command)
    {
        var userId = GetCurrentUserId();
        var item = await _watchlistRepository.GetByUserIdAndIdAsync(userId, command.WatchlistItemId);
        
        if (item == null)
        {
            return Result<bool>.Failure(ErrorMessages.WatchlistItemNotFound);
        }

        item.MarkForRemoval();

        await _watchlistRepository.DeleteAsync(item);
        
        await InvalidateUserStatisticsAsync(userId);
        
        return Result<bool>.Success(true);
    }

    public async Task<WatchlistItem?> GetWatchlistItemByIdAsync(GetMyWatchlistItemByIdQuery query)
    {
        var userId = GetCurrentUserId();
        return await _watchlistRepository.GetByUserIdAndIdAsync(userId, query.WatchlistItemId);
    }

    private async Task InvalidateUserStatisticsAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            user.InvalidateStatistics();
            await _userRepository.UpdateAsync(user);
        }
    }
}

