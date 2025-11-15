using MovieWatchlist.Core.Constants;
using MovieWatchlist.Infrastructure.DTOs;
using MovieWatchlist.Tests.Shared.Infrastructure;

namespace MovieWatchlist.Tests.Shared.TestDataBuilders;

public class TmdbMovieDtoBuilder
{
    private int _tmdbId = TestConstants.Movies.DefaultTmdbId;
    private string _title = TestConstants.Movies.DefaultTitle;
    private string _overview = TestConstants.Movies.DefaultOverview;
    private string? _posterPath = TestConstants.Movies.DefaultPosterPath;
    private string? _backdropPath = TestConstants.Movies.DefaultBackdropPath;
    private string? _releaseDate = "2023-01-01";
    private double _voteAverage = TestConstants.Ratings.DefaultTmdbRating;
    private int _voteCount = TestConstants.Movies.DefaultVoteCount;
    private double _popularity = TestConstants.Movies.DefaultPopularity;
    private TmdbGenreDto[] _genres = Array.Empty<TmdbGenreDto>();

    public TmdbMovieDtoBuilder WithTmdbId(int tmdbId)
    {
        _tmdbId = tmdbId;
        return this;
    }

    public TmdbMovieDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public TmdbMovieDtoBuilder WithOverview(string overview)
    {
        _overview = overview;
        return this;
    }

    public TmdbMovieDtoBuilder WithPosterPath(string? posterPath)
    {
        _posterPath = posterPath;
        return this;
    }

    public TmdbMovieDtoBuilder WithBackdropPath(string? backdropPath)
    {
        _backdropPath = backdropPath;
        return this;
    }

    public TmdbMovieDtoBuilder WithReleaseDate(string? releaseDate)
    {
        _releaseDate = releaseDate;
        return this;
    }

    public TmdbMovieDtoBuilder WithVoteAverage(double voteAverage)
    {
        _voteAverage = voteAverage;
        return this;
    }

    public TmdbMovieDtoBuilder WithVoteCount(int voteCount)
    {
        _voteCount = voteCount;
        return this;
    }

    public TmdbMovieDtoBuilder WithPopularity(double popularity)
    {
        _popularity = popularity;
        return this;
    }

    public TmdbMovieDtoBuilder WithGenres(params TmdbGenreDto[] genres)
    {
        _genres = genres;
        return this;
    }

    public TmdbMovieDtoBuilder WithGenres(params (int Id, string Name)[] genreTuples)
    {
        _genres = genreTuples.Select(g => new TmdbGenreDto { Id = g.Id, Name = g.Name }).ToArray();
        return this;
    }

    public TmdbMovieDtoBuilder WithNoGenres()
    {
        _genres = Array.Empty<TmdbGenreDto>();
        return this;
    }

    public TmdbMovieDto Build()
    {
        return new TmdbMovieDto
        {
            TmdbId = _tmdbId,
            Title = _title,
            Overview = _overview,
            PosterPath = _posterPath,
            BackdropPath = _backdropPath,
            ReleaseDate = _releaseDate,
            VoteAverage = _voteAverage,
            VoteCount = _voteCount,
            Popularity = _popularity,
            Genres = _genres
        };
    }
}

