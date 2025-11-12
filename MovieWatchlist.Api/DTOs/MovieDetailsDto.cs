using System.Text.Json;

namespace MovieWatchlist.Api.DTOs;

public record MovieDetailsDto(
    int TmdbId,
    string Title,
    string Overview,
    string PosterPath,
    string? BackdropPath,
    DateTime ReleaseDate,
    double VoteAverage,
    int VoteCount,
    double Popularity,
    string[] Genres,
    JsonElement CreditsJson,
    JsonElement VideosJson);
