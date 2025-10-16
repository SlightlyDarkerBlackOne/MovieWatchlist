using System.Collections.Generic;
using System.Threading.Tasks;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Core.Interfaces;

public interface IMovieRepository : IRepository<Movie>
{
    Task<IEnumerable<Movie>> SearchMoviesAsync(string query);
    Task<Movie?> GetByTmdbIdAsync(int tmdbId);
    Task<IEnumerable<Movie>> GetPopularMoviesAsync();
    Task<IEnumerable<Movie>> GetMoviesByGenreAsync(string genre);
} 