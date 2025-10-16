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
/// Repository for Movie entity with domain-specific operations.
/// Extends the generic repository with movie-specific queries and TMDB operations.
/// </summary>
public class MovieRepository : EfRepository<Movie>, IMovieRepository
{
    public MovieRepository(MovieWatchlistDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Finds a movie by its TMDB ID asynchronously.
    /// </summary>
    /// <param name="tmdbId">The TMDB ID to search for</param>
    /// <returns>The movie if found, null otherwise</returns>
    public async Task<Movie?> GetByTmdbIdAsync(int tmdbId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(m => m.TmdbId == tmdbId);
    }

    /// <summary>
    /// Searches for movies by query asynchronously.
    /// </summary>
    /// <param name="query">The search query</param>
    /// <returns>A collection of movies matching the query</returns>
    public async Task<IEnumerable<Movie>> SearchMoviesAsync(string query)
    {
        return await _dbSet
            .Where(m => m.Title.Contains(query))
            .OrderBy(m => m.Title)
            .Take(50)
            .ToListAsync();
    }

    /// <summary>
    /// Gets movies released in a specific year asynchronously.
    /// </summary>
    /// <param name="year">The release year to filter by</param>
    /// <param name="limit">Maximum number of results to return (default: 100)</param>
    /// <returns>A collection of movies released in the specified year</returns>
    public async Task<IEnumerable<Movie>> GetByReleaseYearAsync(int year, int limit = 100)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = new DateTime(year, 12, 31);

        return await _dbSet
            .Where(m => m.ReleaseDate >= startDate && m.ReleaseDate <= endDate)
            .OrderByDescending(m => m.Popularity)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Gets popular movies asynchronously.
    /// </summary>
    /// <returns>A collection of popular movies</returns>
    public async Task<IEnumerable<Movie>> GetPopularMoviesAsync()
    {
        return await _dbSet
            .OrderByDescending(m => m.Popularity)
            .Take(20)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the highest rated movies asynchronously.
    /// </summary>
    /// <param name="minimumVoteCount">Minimum number of votes required (default: 100)</param>
    /// <param name="limit">Maximum number of results to return (default: 20)</param>
    /// <returns>A collection of the highest rated movies</returns>
    public async Task<IEnumerable<Movie>> GetHighestRatedAsync(int minimumVoteCount = 100, int limit = 20)
    {
        return await _dbSet
            .Where(m => m.VoteCount >= minimumVoteCount)
            .OrderByDescending(m => m.VoteAverage)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Gets movies by genre asynchronously.
    /// </summary>
    /// <param name="genre">The genre to filter by</param>
    /// <returns>A collection of movies in the specified genre</returns>
    public async Task<IEnumerable<Movie>> GetMoviesByGenreAsync(string genre)
    {
        return await _dbSet
            .Where(m => m.Genres.Contains(genre))
            .OrderByDescending(m => m.Popularity)
            .Take(50)
            .ToListAsync();
    }

    /// <summary>
    /// Checks if a movie with the specified TMDB ID already exists.
    /// </summary>
    /// <param name="tmdbId">The TMDB ID to check</param>
    /// <returns>True if the movie exists, false otherwise</returns>
    public async Task<bool> ExistsByTmdbIdAsync(int tmdbId)
    {
        return await _dbSet.AnyAsync(m => m.TmdbId == tmdbId);
    }

    /// <summary>
    /// Updates movie data from TMDB asynchronously.
    /// </summary>
    /// <param name="movie">The movie entity with updated data</param>
    /// <returns>True if the movie was found and updated, false otherwise</returns>
    public async Task<bool> UpdateFromTmdbAsync(Movie movie)
    {
        var existingMovie = await _dbSet.FindAsync(movie.Id);
        if (existingMovie == null)
            return false;

        // Update TMDB-specific fields
        existingMovie.Title = movie.Title;
        existingMovie.Overview = movie.Overview;
        existingMovie.PosterPath = movie.PosterPath;
        existingMovie.ReleaseDate = movie.ReleaseDate;
        existingMovie.VoteAverage = movie.VoteAverage;
        existingMovie.VoteCount = movie.VoteCount;
        existingMovie.Popularity = movie.Popularity;
        existingMovie.Genres = movie.Genres;
        existingMovie.UpdatedAt = DateTime.UtcNow;

        _dbSet.Update(existingMovie);
        return true;
    }
}


