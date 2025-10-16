using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MovieWatchlist.Core.Configuration;
using MovieWatchlist.Core.DTOs;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Infrastructure.Services;

public class TmdbService : ITmdbService
{
    private readonly HttpClient _httpClient;
    private readonly TmdbSettings _settings;
    private readonly IGenreService _genreService;

    private readonly string _baseUrl = "https://api.themoviedb.org/3";

    public TmdbService(HttpClient httpClient, IOptions<TmdbSettings> settings, IGenreService genreService)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _genreService = genreService;
    }

    public async Task<IEnumerable<Movie>> SearchMoviesAsync(string query, int page = 1)
    {
        var url = $"{_baseUrl}/search/movie?api_key={_settings.ApiKey}&query={Uri.EscapeDataString(query)}&page={page}";
        var response = await _httpClient.GetFromJsonAsync<TmdbSearchResponse>(url);

        var movies = MapToMovies(response?.Results ?? Array.Empty<TmdbMovieDto>());
        
        // Sort by vote count descending to show most popular/relevant movies first
        return movies.OrderByDescending(m => m.VoteCount);
    }

    public async Task<Movie?> GetMovieDetailsAsync(int tmdbId)
    {
        // Use append_to_response to get movie details, credits, and videos in ONE API call
        var url = $"{_baseUrl}/movie/{tmdbId}?api_key={_settings.ApiKey}&append_to_response=credits,videos";
        
        // Retry logic for rate limiting
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
                
                // Cache credits and videos data from the single API call
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
                    // Log error but don't fail - movie data is still valid without credits/videos
                    Console.WriteLine($"Failed to serialize credits/videos for movie {tmdbId}: {ex.Message}");
                }
                
                return movie;
            }
            
            // Handle rate limiting (429) with exponential backoff
            if (httpResponse.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                if (attempt < maxRetries - 1)
                {
                    var delay = retryDelayMs * (int)Math.Pow(2, attempt);
                    Console.WriteLine($"Rate limited. Retrying in {delay}ms... (Attempt {attempt + 1}/{maxRetries})");
                    await Task.Delay(delay);
                    continue;
                }
            }
            
            var content = await httpResponse.Content.ReadAsStringAsync();
            throw new HttpRequestException($"TMDB API request failed with status {httpResponse.StatusCode}: {content}");
        }
        
        return null;
    }

    public async Task<IEnumerable<Movie>> GetPopularMoviesAsync(int page = 1)
    {
        var random = new Random();
        var randomPage = random.Next(1, 4); // Random page between 1 and 3
        
        // Randomly select which endpoint to use for variety
        var endpointChoice = random.Next(0, 3);
        string endpoint = endpointChoice switch
        {
            0 => "movie/popular",      // Popular movies
            1 => "movie/top_rated",    // Top rated movies
            _ => "movie/now_playing"   // Now playing in theaters
        };
        
        var url = $"{_baseUrl}/{endpoint}?api_key={_settings.ApiKey}&page={randomPage}";
        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"TMDB API request failed with status {response.StatusCode}: {content}");
        }

        var tmdbResponse = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>();
        var movies = MapToMovies(tmdbResponse?.Results ?? Array.Empty<TmdbMovieDto>()).ToList();
        
        // Shuffle the results to add even more variety
        return movies.OrderBy(m => random.Next()).ToList();
    }

    public async Task<IEnumerable<Movie>> GetMoviesByGenreAsync(string genre, int page = 1)
    {
        var genreId = _genreService.GetGenreId(genre);
        if (genreId == null)
        {
            throw new ArgumentException($"Invalid genre: {genre}. Please use a valid genre name like 'comedy', 'action', 'drama', etc.");
        }

        // Use absolute URL since base address is commented out
        var url = $"{_baseUrl}/discover/movie?api_key={_settings.ApiKey}&with_genres={genreId}&page={page}";
        Console.WriteLine($"Making request to: {url}");
        Console.WriteLine($"API Key: {_settings.ApiKey}");
        
        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"TMDB API request failed with status {response.StatusCode}: {content}");
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
            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"TMDB API request failed with status {response.StatusCode}: {content}");
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
            var content = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"TMDB API request failed with status {response.StatusCode}: {content}");
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
            // Ensure the DateTime has UTC kind for PostgreSQL compatibility
            releaseDate = DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }
        else
        {
            releaseDate = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
        }

        // Use genre names from TMDB detailed movie response
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