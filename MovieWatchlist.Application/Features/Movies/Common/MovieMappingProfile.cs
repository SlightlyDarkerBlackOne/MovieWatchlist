using System.Text.Json;
using Mapster;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Movies.Common;

public class MovieMappingProfile : IRegister
{
    private const string EmptyCreditsJson = "{\"cast\":[],\"crew\":[]}";
    private const string EmptyVideosJson = "[]";

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Movie, MovieDetailsDto>()
            .Map(dest => dest.TmdbId, src => src.TmdbId)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Overview, src => src.Overview)
            .Map(dest => dest.PosterPath, src => src.PosterPath)
            .Map(dest => dest.BackdropPath, src => src.BackdropPath)
            .Map(dest => dest.ReleaseDate, src => src.ReleaseDate)
            .Map(dest => dest.VoteAverage, src => src.VoteAverage)
            .Map(dest => dest.VoteCount, src => src.VoteCount)
            .Map(dest => dest.Popularity, src => src.Popularity)
            .Map(dest => dest.Genres, src => src.Genres)
            .Map(dest => dest.CreditsJson, src => ParseJsonOrDefault(src.CreditsJson, EmptyCreditsJson))
            .Map(dest => dest.VideosJson, src => ParseJsonOrDefault(src.VideosJson, EmptyVideosJson));
    }

    private static JsonElement ParseJsonOrDefault(string? json, string defaultJson)
    {
        var payload = string.IsNullOrWhiteSpace(json) ? defaultJson : json!;
        using var document = JsonDocument.Parse(payload);
        return document.RootElement.Clone();
    }
}

