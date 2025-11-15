using System.Text.Json;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Tests.Shared.Infrastructure;

namespace MovieWatchlist.Tests.Shared.TestDataBuilders;

/// <summary>
/// Builder for creating MovieDetailsDto test data
/// </summary>
public class MovieDetailsDtoBuilder
{
    private int _tmdbId = TestConstants.Movies.DefaultTmdbId;
    private string _title = TestConstants.Movies.DefaultTitle;
    private string _overview = TestConstants.Movies.DefaultOverview;
    private string _posterPath = TestConstants.Movies.DefaultPosterPath;
    private string? _backdropPath = TestConstants.Movies.DefaultBackdropPath;
    private DateTime _releaseDate = DateTime.UtcNow;
    private double _voteAverage = TestConstants.Ratings.DefaultTmdbRating;
    private int _voteCount = 100;
    private double _popularity = TestConstants.Movies.DefaultPopularity;
    private string[] _genres = Array.Empty<string>();
    private JsonElement _creditsJson;
    private JsonElement _videosJson;

    public MovieDetailsDtoBuilder()
    {
        _creditsJson = JsonDocument.Parse("{\"cast\":[],\"crew\":[]}").RootElement;
        _videosJson = JsonDocument.Parse("[]").RootElement;
    }

    public MovieDetailsDtoBuilder WithTmdbId(int tmdbId)
    {
        _tmdbId = tmdbId;
        return this;
    }

    public MovieDetailsDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public MovieDetailsDtoBuilder WithOverview(string overview)
    {
        _overview = overview;
        return this;
    }

    public MovieDetailsDtoBuilder WithPosterPath(string posterPath)
    {
        _posterPath = posterPath;
        return this;
    }

    public MovieDetailsDtoBuilder WithBackdropPath(string? backdropPath)
    {
        _backdropPath = backdropPath;
        return this;
    }

    public MovieDetailsDtoBuilder WithReleaseDate(DateTime releaseDate)
    {
        _releaseDate = releaseDate;
        return this;
    }

    public MovieDetailsDtoBuilder WithVoteAverage(double voteAverage)
    {
        _voteAverage = voteAverage;
        return this;
    }

    public MovieDetailsDtoBuilder WithVoteCount(int voteCount)
    {
        _voteCount = voteCount;
        return this;
    }

    public MovieDetailsDtoBuilder WithPopularity(double popularity)
    {
        _popularity = popularity;
        return this;
    }

    public MovieDetailsDtoBuilder WithGenres(params string[] genres)
    {
        _genres = genres;
        return this;
    }

    public MovieDetailsDtoBuilder WithCreditsJson(string json)
    {
        _creditsJson = JsonDocument.Parse(json).RootElement;
        return this;
    }

    public MovieDetailsDtoBuilder WithVideosJson(string json)
    {
        _videosJson = JsonDocument.Parse(json).RootElement;
        return this;
    }

    public MovieDetailsDto Build()
    {
        return new MovieDetailsDto(
            _tmdbId,
            _title,
            _overview,
            _posterPath,
            _backdropPath,
            _releaseDate,
            _voteAverage,
            _voteCount,
            _popularity,
            _genres,
            _creditsJson,
            _videosJson
        );
    }
}

