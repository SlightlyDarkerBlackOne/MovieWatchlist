using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MovieWatchlist.Infrastructure.Configuration;
using MovieWatchlist.Infrastructure.DTOs;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Exceptions;
using MovieWatchlist.Core.Constants;

namespace MovieWatchlist.Infrastructure.Services;

public class TmdbService : ITmdbService
{
    private readonly HttpClient _httpClient;
    private readonly TmdbSettings _settings;
    private readonly IGenreService _genreService;
    private readonly ILogger<TmdbService> _logger;

    private readonly string _baseUrl = "https://api.themoviedb.org/3";

    public TmdbService(HttpClient httpClient, IOptions<TmdbSettings> settings, IGenreService genreService, ILogger<TmdbService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _genreService = genreService;
        _logger = logger;
    }

    /// <summary>
    /// Searches for movies by query string.
    /// </summary>
    public async Task<IEnumerable<Movie>> SearchMoviesAsync(string query, int page = 1)
    {
        var url = $"{_baseUrl}/search/movie?api_key={_settings.ApiKey}&query={Uri.EscapeDataString(query)}&page={page}";
        var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(url);

        var movies = MapToMovies(response?.Results ?? Array.Empty<TmdbMovieDto>());
        
        return movies;
    }

    /// <summary>
    /// Gets detailed movie information from TMDB API. Uses append_to_response to fetch
    /// movie details, credits, and videos in a single API call for efficiency.
    /// Implements retry logic with exponential backoff for rate limiting (429 errors).
    /// </summary>
    public async Task<Movie?> GetMovieDetailsAsync(int tmdbId)
    {
        var url = $"{_baseUrl}/movie/{tmdbId}?api_key={_settings.ApiKey}&append_to_response=credits,videos";
        
        int maxRetries = 3;
        int retryDelayMs = 1000;
        
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            var httpResponse = await _httpClient.GetAsync(url);

            if (httpResponse.IsSuccessStatusCode)
            {
                var response = await httpResponse.Content.ReadFromJsonAsync<TmdbMovieDto>();
                if (response == null) return null;

                var movie = MapToMovie(response);
                
                try
                {
                    if (response.Credits != null)
                    {
                        movie.CreditsJson = System.Text.Json.JsonSerializer.Serialize(response.Credits);
                    }
                    
                    if (response.Videos != null)
                    {
                        movie.VideosJson = System.Text.Json.JsonSerializer.Serialize(response.Videos.Results);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to serialize credits/videos for movie {TmdbId}", tmdbId);
                }
                
                return movie;
            }
            
            if (httpResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                if (attempt < maxRetries - 1)
                {
                    var delay = retryDelayMs * (int)Math.Pow(2, attempt);
                    _logger.LogWarning("Rate limited. Retrying in {Delay}ms... (Attempt {Attempt}/{MaxRetries})", delay, attempt + 1, maxRetries);
                    await Task.Delay(delay);
                    continue;
                }
                throw new RateLimitException(ErrorMessages.TmdbRateLimitExceeded, retryDelayMs);
            }
            
            var content = await httpResponse.Content.ReadAsStringAsync();
            throw new ExternalServiceException($"TMDB API request failed: {content}", httpResponse.StatusCode);
        }
        
        return null;
    }

    /// <summary>
    /// Gets popular movies from TMDB. Randomly selects between popular, top-rated, and now-playing
    /// endpoints for variety. Results are shuffled before returning.
    /// </summary>
    public async Task<IEnumerable<Movie>> GetPopularMoviesAsync(int page = 1)
    {
        var random = new Random();
        var randomPage = random.Next(1, 4);
        
        var endpointChoice = random.Next(0, 3);
        string endpoint = endpointChoice switch
        {
            0 => "movie/popular",
            1 => "movie/top_rated",
            _ => "movie/now_playing"
        };
        
        var url = $"{_baseUrl}/{endpoint}?api_key={_settings.ApiKey}&page={randomPage}";
        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new RateLimitException(ErrorMessages.TmdbRateLimitExceeded, 1000);
            }
            
            var content = await response.Content.ReadAsStringAsync();
            throw new ExternalServiceException($"TMDB API request failed: {content}", response.StatusCode);
        }

        var tmdbResponse = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>();
        var movies = MapToMovies(tmdbResponse?.Results ?? Array.Empty<TmdbMovieDto>()).ToList();
        
        return movies.OrderBy(m => random.Next()).ToList();
    }

    public async Task<IEnumerable<Movie>> GetMoviesByGenreAsync(string genre, int page = 1)
    {
        var genreId = _genreService.GetGenreId(genre);
        if (genreId == null)
        {
            throw new ArgumentException($"Invalid genre: {genre}. Please use a valid genre name like 'comedy', 'action', 'drama', etc.");
        }

        var url = $"{_baseUrl}/discover/movie?api_key={_settings.ApiKey}&with_genres={genreId}&page={page}";
        
        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new RateLimitException(ErrorMessages.TmdbRateLimitExceeded, 1000);
            }
            
            var content = await response.Content.ReadAsStringAsync();
            throw new ExternalServiceException($"TMDB API request failed: {content}", response.StatusCode);
        }

        var tmdbResponse = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>();
        return MapToMovies(tmdbResponse?.Results ?? Array.Empty<TmdbMovieDto>());
    }

    public async Task<IEnumerable<object>> GetMovieVideosAsync(int tmdbId)
    {
        var url = $"{_baseUrl}/movie/{tmdbId}/videos?api_key={_settings.ApiKey}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new RateLimitException(ErrorMessages.TmdbRateLimitExceeded, 1000);
            }
            
            var content = await response.Content.ReadAsStringAsync();
            throw new ExternalServiceException($"TMDB API request failed: {content}", response.StatusCode);
        }

        var videosResponse = await response.Content.ReadFromJsonAsync<TmdbVideosResponse>();
        return videosResponse?.Results ?? new List<object>();
    }

    public async Task<object> GetMovieCreditsAsync(int tmdbId)
    {
        var url = $"{_baseUrl}/movie/{tmdbId}/credits?api_key={_settings.ApiKey}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new RateLimitException(ErrorMessages.TmdbRateLimitExceeded, 1000);
            }
            
            var content = await response.Content.ReadAsStringAsync();
            throw new ExternalServiceException($"TMDB API request failed: {content}", response.StatusCode);
        }

        var credits = await response.Content.ReadFromJsonAsync<object>();
        return credits ?? new object();
    }

    public string GetPosterUrl(string posterPath, string size = "w500")
    {
        return $"{_settings.ImageBaseUrl}/{size}{posterPath}";
    }

    private IEnumerable<Movie> MapToMovies(IEnumerable<TmdbMovieDto> dtos)
    {
        return dtos.Select(MapToMovie);
    }

    private Movie MapToMovie(TmdbMovieDto dto)
    {
        DateTime releaseDate = DateTime.MinValue;
        if (DateTime.TryParse(dto.ReleaseDate, out var date))
        {
            releaseDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }
        else
        {
            releaseDate = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
        }

        var genreNames = dto.Genres.Select(g => g.Name).ToArray();

        return new Movie
        {
            TmdbId = dto.TmdbId,
            Title = dto.Title,
            Overview = dto.Overview,
            PosterPath = dto.PosterPath ?? string.Empty,
            BackdropPath = dto.BackdropPath,
            ReleaseDate = releaseDate,
            VoteAverage = dto.VoteAverage,
            VoteCount = dto.VoteCount,
            Popularity = dto.Popularity,
            Genres = genreNames
        };
    }
} 