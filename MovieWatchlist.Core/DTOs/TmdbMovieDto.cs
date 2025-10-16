using System;
using System.Text.Json.Serialization;

namespace MovieWatchlist.Core.DTOs;

public class TmdbMovieDto
{
    [JsonPropertyName("id")]
    public int TmdbId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("overview")]
    public string Overview { get; set; } = string.Empty;

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("backdrop_path")]
    public string? BackdropPath { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; set; }

    [JsonPropertyName("vote_count")]
    public int VoteCount { get; set; }

    [JsonPropertyName("popularity")]
    public double Popularity { get; set; }

    [JsonPropertyName("genres")]
    public TmdbGenreDto[] Genres { get; set; } = Array.Empty<TmdbGenreDto>();

    [JsonPropertyName("credits")]
    public TmdbCreditsResponse? Credits { get; set; }

    [JsonPropertyName("videos")]
    public TmdbVideosResponse? Videos { get; set; }
}

public class TmdbSearchResponse
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("results")]
    public TmdbMovieDto[] Results { get; set; } = Array.Empty<TmdbMovieDto>();

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("total_results")]
    public int TotalResults { get; set; }
}

public class TmdbCreditsResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("cast")]
    public List<object> Cast { get; set; } = new();

    [JsonPropertyName("crew")]
    public List<object> Crew { get; set; } = new();
}

public class TmdbVideosResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("results")]
    public List<object> Results { get; set; } = new();
}

public class TmdbGenreDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
} 