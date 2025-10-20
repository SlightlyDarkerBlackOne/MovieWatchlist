using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Exceptions;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Queries;
using MovieWatchlist.Core.Specifications;
using MovieWatchlist.Core.ValueObjects;

namespace MovieWatchlist.Application.Services;

public class WatchlistService : IWatchlistService
{
    private readonly IWatchlistRepository m_watchlistRepository;
    private readonly IMovieRepository m_movieRepository;
    private readonly IUserRepository m_userRepository;
    private readonly ITmdbService m_tmdbService;

    public WatchlistService(
        IWatchlistRepository watchlistRepository,
        IMovieRepository movieRepository,
        IUserRepository userRepository,
        ITmdbService tmdbService)
    {
        m_watchlistRepository = watchlistRepository;
        m_movieRepository = movieRepository;
        m_userRepository = userRepository;
        m_tmdbService = tmdbService;
    }

    public async Task<IEnumerable<WatchlistItem>> GetUserWatchlistAsync(GetUserWatchlistQuery query)
    {
        return await m_watchlistRepository.GetByUserIdAsync(query.UserId);
    }

    public async Task<IEnumerable<WatchlistItem>> GetWatchlistByStatusAsync(GetWatchlistByStatusQuery query)
    {
        return await m_watchlistRepository.GetByUserIdAndStatusAsync(query.UserId, query.Status);
    }

    public async Task<IEnumerable<WatchlistItem>> GetFavoriteMoviesAsync(GetFavoriteMoviesQuery query)
    {
        return await m_watchlistRepository.GetFavoritesByUserIdAsync(query.UserId);
    }

    public async Task<WatchlistStatistics> GetUserStatisticsAsync(GetUserStatisticsQuery query)
    {
        var user = await m_userRepository.GetByIdAsync(query.UserId);
        if (user == null)
            throw new ApiException($"User with ID {query.UserId} not found");

        // Return cached statistics if available
        var cachedStats = user.GetCachedStatistics();
        if (cachedStats != null)
            return cachedStats;

        // Calculate and cache new statistics
        var watchlist = await m_watchlistRepository.GetByUserIdAsync(query.UserId);
        var statistics = user.CalculateStatistics(watchlist);
        
        user.UpdateCachedStatistics(statistics);
        await m_userRepository.UpdateAsync(user);
        
        return statistics;
    }

    public async Task<IEnumerable<Movie>> GetRecommendedMoviesAsync(GetRecommendedMoviesQuery query)
    {
        var userWatchlist = await m_watchlistRepository.GetByUserIdAsync(query.UserId);
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

        var allMovies = await m_movieRepository.GetAllAsync();
        
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

    public async Task<IEnumerable<WatchlistItem>> GetWatchlistByGenreAsync(GetWatchlistByGenreQuery query)
    {
        var watchlist = await m_watchlistRepository.GetByUserIdAsync(query.UserId);
        var spec = new WatchlistByGenreSpecification(query.Genre);
        return watchlist
            .Where(spec.IsSatisfiedBy)
            .OrderByDescending(w => w.AddedDate);
    }

    public async Task<IEnumerable<WatchlistItem>> GetWatchlistByYearRangeAsync(GetWatchlistByYearRangeQuery query)
    {
        var watchlist = await m_watchlistRepository.GetByUserIdAsync(query.UserId);
        var spec = new WatchlistByYearRangeSpecification(query.StartYear, query.EndYear);
        return watchlist
            .Where(spec.IsSatisfiedBy)
            .OrderByDescending(w => w.Movie.ReleaseDate);
    }

    public async Task<IEnumerable<WatchlistItem>> GetWatchlistByRatingRangeAsync(GetWatchlistByRatingRangeQuery query)
    {
        var watchlist = await m_watchlistRepository.GetByUserIdAsync(query.UserId);
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
        var cachedMovie = await m_movieRepository.GetByTmdbIdAsync(command.MovieId);
        
        Movie movie;
        if (cachedMovie == null)
        {
            var tmdbMovie = await m_tmdbService.GetMovieDetailsAsync(command.MovieId);
            if (tmdbMovie == null)
                return Result<WatchlistItem>.Failure(string.Format(ErrorMessages.MovieNotFound, command.MovieId));
            
            await m_movieRepository.AddAsync(tmdbMovie);
            movie = tmdbMovie;
        }
        else
        {
            movie = cachedMovie;
        }

        var alreadyInWatchlist = await m_watchlistRepository.IsMovieInUserWatchlistAsync(command.UserId, movie.Id);
        if (alreadyInWatchlist)
            return Result<WatchlistItem>.Failure(ErrorMessages.MovieAlreadyInWatchlist);

        var watchlistItem = WatchlistItem.Create(command.UserId, movie.Id, movie);
        
        if (command.Status != WatchlistStatus.Planned)
        {
            watchlistItem.UpdateStatus(command.Status);
        }
        
        if (!string.IsNullOrEmpty(command.Notes))
        {
            watchlistItem.UpdateNotes(command.Notes);
        }

        await m_watchlistRepository.AddAsync(watchlistItem);
        
        await InvalidateUserStatisticsAsync(command.UserId);
        
        return Result<WatchlistItem>.Success(watchlistItem);
    }

    public async Task<Result<WatchlistItem>> UpdateWatchlistItemAsync(UpdateWatchlistItemCommand command)
    {
        var item = await m_watchlistRepository.GetByUserIdAndIdAsync(command.UserId, command.WatchlistItemId);
        
        if (item == null)
            return Result<WatchlistItem>.Failure(ErrorMessages.WatchlistItemNotFound);

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

        await m_watchlistRepository.UpdateAsync(item);
        return Result<WatchlistItem>.Success(item);
    }

    public async Task<Result<bool>> RemoveFromWatchlistAsync(RemoveFromWatchlistCommand command)
    {
        var item = await m_watchlistRepository.GetByUserIdAndIdAsync(command.UserId, command.WatchlistItemId);
        
        if (item == null)
            return Result<bool>.Failure(ErrorMessages.WatchlistItemNotFound);

        await m_watchlistRepository.DeleteAsync(item);
        
        await InvalidateUserStatisticsAsync(command.UserId);
        
        return Result<bool>.Success(true);
    }

    public async Task<WatchlistItem?> GetWatchlistItemByIdAsync(GetWatchlistItemByIdQuery query)
    {
        return await m_watchlistRepository.GetByUserIdAndIdAsync(query.UserId, query.WatchlistItemId);
    }

    private async Task InvalidateUserStatisticsAsync(int userId)
    {
        var user = await m_userRepository.GetByIdAsync(userId);
        if (user != null)
        {
            user.InvalidateStatistics();
            await m_userRepository.UpdateAsync(user);
        }
    }
}

