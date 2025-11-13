using System.Text.Json;

namespace MovieWatchlist.Application.Features.Movies.Common;

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

