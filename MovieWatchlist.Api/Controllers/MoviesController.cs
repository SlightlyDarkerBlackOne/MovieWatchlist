using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using System.Linq;

namespace MovieWatchlist.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly ITmdbService _tmdbService;
    private readonly IRepository<Movie> _movieRepository;

    public MoviesController(ITmdbService tmdbService, IRepository<Movie> movieRepository)
    {
        _tmdbService = tmdbService;
        _movieRepository = movieRepository;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Movie>>> SearchMovies([FromQuery] string query, [FromQuery] int page = 1)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Search query is required");

        var movies = await _tmdbService.SearchMoviesAsync(query, page);
        return Ok(movies);
    }

    [HttpGet("{tmdbId}")]
    public async Task<ActionResult<Movie>> GetMovieDetails(int tmdbId)
    {
        var movie = await _tmdbService.GetMovieDetailsAsync(tmdbId);
        if (movie == null)
            return NotFound();

        return Ok(movie);
    }

    [HttpGet("popular")]
    public async Task<ActionResult<IEnumerable<Movie>>> GetPopularMovies([FromQuery] int page = 1)
    {
        var movies = await _tmdbService.GetPopularMoviesAsync(page);
        return Ok(movies);
    }

    [HttpGet("genre/{genre}")]
    public async Task<ActionResult<IEnumerable<Movie>>> GetMoviesByGenre(string genre, [FromQuery] int page = 1)
    {
        try
        {
            var movies = await _tmdbService.GetMoviesByGenreAsync(genre, page);
            return Ok(movies);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("tmdb/{tmdbId}")]
    public async Task<ActionResult<object>> GetMovieDetailsByTmdbId(int tmdbId)
    {
        try
        {
            // Check if we have cached data first
            var cachedMovies = await _movieRepository.FindAsync(m => m.TmdbId == tmdbId);
            var cachedMovie = cachedMovies.FirstOrDefault();
            
            if (cachedMovie != null && !string.IsNullOrEmpty(cachedMovie.CreditsJson) && !string.IsNullOrEmpty(cachedMovie.VideosJson))
            {
                // Return all data from cache in one response
                var response = new
                {
                    movie = cachedMovie,
                    credits = System.Text.Json.JsonSerializer.Deserialize<object>(cachedMovie.CreditsJson),
                    videos = System.Text.Json.JsonSerializer.Deserialize<object>(cachedMovie.VideosJson)
                };
                return Ok(response);
            }
            
            // Fallback: Fetch from TMDB and cache
            var movie = await _tmdbService.GetMovieDetailsAsync(tmdbId);
            if (movie == null)
                return NotFound($"Movie with TMDB ID {tmdbId} not found");
                
            // Save to cache
            await _movieRepository.AddAsync(movie);
            
            var fallbackResponse = new
            {
                movie = movie,
                credits = !string.IsNullOrEmpty(movie.CreditsJson) 
                    ? System.Text.Json.JsonSerializer.Deserialize<object>(movie.CreditsJson) 
                    : new { cast = new object[0], crew = new object[0] },
                videos = !string.IsNullOrEmpty(movie.VideosJson)
                    ? System.Text.Json.JsonSerializer.Deserialize<object>(movie.VideosJson)
                    : new object[0]
            };
            
            return Ok(fallbackResponse);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("429"))
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new { message = "TMDB API rate limit exceeded. Please try again in a moment.", details = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to fetch movie data", details = ex.Message });
        }
    }
} 