using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MovieWatchlist.Core.Commands;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Queries;

namespace MovieWatchlist.Application.Services;

public class WatchlistService : IWatchlistService
{
    private readonly IWatchlistRepository m_watchlistRepository;
    private readonly IMovieRepository m_movieRepository;
    private readonly ITmdbService m_tmdbService;
    private readonly IUnitOfWork m_unitOfWork;

    public WatchlistService(
        IWatchlistRepository watchlistRepository,
        IMovieRepository movieRepository,
        ITmdbService tmdbService,
        IUnitOfWork unitOfWork)
    {
        m_watchlistRepository = watchlistRepository;
        m_movieRepository = movieRepository;
        m_tmdbService = tmdbService;
        m_unitOfWork = unitOfWork;
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
        var watchlist = await m_watchlistRepository.GetByUserIdAsync(query.UserId);
        var watchlistList = watchlist.ToList();

        var totalMovies = watchlistList.Count;
        var watchedMovies = watchlistList.Count(w => w.Status == WatchlistStatus.Watched);
        var plannedMovies = watchlistList.Count(w => w.Status == WatchlistStatus.Planned);
        var favoriteMovies = watchlistList.Count(w => w.IsFavorite);
        var moviesThisYear = watchlistList.Count(w => w.AddedDate.Year == DateTime.UtcNow.Year);

        // Calculate average ratings using LINQ
        var ratedMovies = watchlistList.Where(w => w.UserRating.HasValue).ToList();
        var averageUserRating = ratedMovies.Any() ? ratedMovies.Average(w => w.UserRating!.Value) : 0;
        var averageTmdbRating = watchlistList.Any() ? watchlistList.Average(w => w.Movie.VoteAverage) : 0;

        // Genre breakdown using LINQ GroupBy
        var genreGroups = watchlistList
            .Where(w => w.Status == WatchlistStatus.Watched)
            .SelectMany(w => w.Movie.Genres)
            .GroupBy(g => g)
            .OrderByDescending(g => g.Count());

        var genreBreakdown = genreGroups.ToDictionary(g => g.Key, g => g.Count());
        var mostWatchedGenre = genreGroups.FirstOrDefault()?.Key ?? string.Empty;

        // Yearly breakdown using LINQ GroupBy
        var yearlyGroups = watchlistList
            .Where(w => w.Status == WatchlistStatus.Watched)
            .GroupBy(w => w.Movie.ReleaseDate.Year)
            .OrderByDescending(g => g.Key);

        var yearlyBreakdown = yearlyGroups.ToDictionary(g => g.Key, g => g.Count());

        return new WatchlistStatistics(
            TotalMovies: totalMovies,
            WatchedMovies: watchedMovies,
            PlannedMovies: plannedMovies,
            FavoriteMovies: favoriteMovies,
            AverageUserRating: averageUserRating,
            AverageTmdbRating: averageTmdbRating,
            MostWatchedGenre: mostWatchedGenre,
            MoviesThisYear: moviesThisYear,
            GenreBreakdown: genreBreakdown,
            YearlyBreakdown: yearlyBreakdown
        );
    }

    public async Task<IEnumerable<Movie>> GetRecommendedMoviesAsync(GetRecommendedMoviesQuery query)
    {
        var userWatchlist = await m_watchlistRepository.GetByUserIdAsync(query.UserId);
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
        var allMovies = await m_movieRepository.GetAllAsync();
        
        var recommendedMovies = allMovies
            .Where(m => !userWatchlistList.Any(w => w.MovieId == m.Id)) // Not in user's watchlist
            .Where(m => m.Genres.Any(g => favoriteGenres.Contains(g))) // Matches favorite genres
            .Where(m => m.VoteAverage >= 7.0) // High TMDB rating
            .Where(m => m.VoteCount >= 1000) // Sufficient votes
            .OrderByDescending(m => m.Popularity)
            .ThenByDescending(m => m.VoteAverage)
            .Take(query.Limit);

        return recommendedMovies;
    }

    public async Task<IEnumerable<WatchlistItem>> GetWatchlistByGenreAsync(GetWatchlistByGenreQuery query)
    {
        var watchlist = await m_watchlistRepository.GetByUserIdAsync(query.UserId);
        return watchlist
            .Where(w => w.Movie.Genres.Contains(query.Genre))
            .OrderByDescending(w => w.AddedDate);
    }

    public async Task<IEnumerable<WatchlistItem>> GetWatchlistByYearRangeAsync(GetWatchlistByYearRangeQuery query)
    {
        var watchlist = await m_watchlistRepository.GetByUserIdAsync(query.UserId);
        return watchlist
            .Where(w => w.Movie.ReleaseDate.Year >= query.StartYear && w.Movie.ReleaseDate.Year <= query.EndYear)
            .OrderByDescending(w => w.Movie.ReleaseDate);
    }

    public async Task<IEnumerable<WatchlistItem>> GetWatchlistByRatingRangeAsync(GetWatchlistByRatingRangeQuery query)
    {
        var watchlist = await m_watchlistRepository.GetByUserIdAsync(query.UserId);
        return watchlist
            .Where(w => w.Movie.VoteAverage >= query.MinRating && w.Movie.VoteAverage <= query.MaxRating)
            .OrderByDescending(w => w.Movie.VoteAverage);
    }


    public async Task<WatchlistItem> AddToWatchlistAsync(AddToWatchlistCommand command)
    {
        // movieId parameter is the TMDB ID - check if we already have this movie cached in our database
        var cachedMovie = await m_movieRepository.GetByTmdbIdAsync(command.MovieId);
        
        Movie movie;
        if (cachedMovie == null)
        {
            // Not in cache - fetch from TMDB and save to database
            var tmdbMovie = await m_tmdbService.GetMovieDetailsAsync(command.MovieId);
            if (tmdbMovie == null)
                throw new ArgumentException(string.Format(ErrorMessages.MovieNotFound, command.MovieId));
            
            // Save to database as cache and persist immediately to get the database ID
            await m_movieRepository.AddAsync(tmdbMovie);
            await m_unitOfWork.SaveChangesAsync();
            movie = tmdbMovie;
        }
        else
        {
            // Already cached - use it
            movie = cachedMovie;
        }

        // Check if already in user's watchlist
        var alreadyInWatchlist = await m_watchlistRepository.IsMovieInUserWatchlistAsync(command.UserId, movie.Id);
        if (alreadyInWatchlist)
            throw new InvalidOperationException(ErrorMessages.MovieAlreadyInWatchlist);

        // Use factory method to create watchlist item
        var watchlistItem = WatchlistItem.Create(command.UserId, movie.Id, movie);
        
        // Apply initial status if not default
        if (command.Status != WatchlistStatus.Planned)
        {
            watchlistItem.UpdateStatus(command.Status);
        }
        
        // Add notes if provided
        if (!string.IsNullOrEmpty(command.Notes))
        {
            watchlistItem.UpdateNotes(command.Notes);
        }

        await m_watchlistRepository.AddAsync(watchlistItem);
        await m_unitOfWork.SaveChangesAsync();
        return watchlistItem;
    }

    public async Task<WatchlistItem?> UpdateWatchlistItemAsync(UpdateWatchlistItemCommand command)
    {
        var item = await m_watchlistRepository.GetByUserIdAndIdAsync(command.UserId, command.WatchlistItemId);
        
        if (item == null)
            return null;

        // Update properties using domain methods
        if (command.Status.HasValue)
        {
            item.UpdateStatus(command.Status.Value);
        }

        if (command.IsFavorite.HasValue)
        {
            // Only toggle if the value differs from current
            if (item.IsFavorite != command.IsFavorite.Value)
            {
                item.ToggleFavorite();
            }
        }

        if (command.UserRating.HasValue)
        {
            item.SetRating(command.UserRating.Value);
        }

        if (command.Notes != null)
        {
            item.UpdateNotes(command.Notes);
        }

        // Note: WatchedDate is now automatically managed by UpdateStatus method
        // when status is set to Watched. Direct manipulation is no longer allowed.

        await m_watchlistRepository.UpdateAsync(item);
        await m_unitOfWork.SaveChangesAsync();
        return item;
    }

    public async Task<bool> RemoveFromWatchlistAsync(RemoveFromWatchlistCommand command)
    {
        var item = await m_watchlistRepository.GetByUserIdAndIdAsync(command.UserId, command.WatchlistItemId);
        
        if (item == null)
            return false;

        await m_watchlistRepository.DeleteAsync(item);
        await m_unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<WatchlistItem?> GetWatchlistItemByIdAsync(GetWatchlistItemByIdQuery query)
    {
        return await m_watchlistRepository.GetByUserIdAndIdAsync(query.UserId, query.WatchlistItemId);
    }
}

