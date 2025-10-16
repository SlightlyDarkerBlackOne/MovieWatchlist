using System.Collections.Generic;
using System.Threading.Tasks;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Core.Interfaces;

public interface ITmdbService
{
    Task<IEnumerable<Movie>> SearchMoviesAsync(string query, int page = 1);
    Task<Movie?> GetMovieDetailsAsync(int tmdbId);
    Task<IEnumerable<Movie>> GetPopularMoviesAsync(int page = 1);
    Task<IEnumerable<Movie>> GetMoviesByGenreAsync(string genre, int page = 1);
    Task<IEnumerable<object>> GetMovieVideosAsync(int tmdbId);
    Task<object> GetMovieCreditsAsync(int tmdbId);
    string GetPosterUrl(string posterPath, string size = "w500");
} 