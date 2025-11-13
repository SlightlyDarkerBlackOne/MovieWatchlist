using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Tests.Infrastructure;

namespace MovieWatchlist.Tests.TestDataBuilders;

/// <summary>
/// Builder for creating Movie test data
/// </summary>
public class MovieBuilder
{
    private readonly Movie _movie;

    public MovieBuilder()
    {
        _movie = new Movie
        {
            TmdbId = TestConstants.Movies.DefaultTmdbId,
            Title = TestConstants.Movies.DefaultTitle,
            Overview = TestConstants.Movies.DefaultOverview,
            ReleaseDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            VoteAverage = TestConstants.Ratings.DefaultTmdbRating,
            VoteCount = TestConstants.Movies.DefaultVoteCount,
            Popularity = TestConstants.Movies.DefaultPopularity,
            PosterPath = TestConstants.Movies.DefaultPosterPath,
            Genres = new[] { GenreConstants.Action, GenreConstants.Drama }
        };
    }

    public MovieBuilder WithId(int id)
    {
        typeof(Movie).GetProperty("Id")!.SetValue(_movie, id);
        return this;
    }

    public MovieBuilder WithTmdbId(int tmdbId)
    {
        _movie.TmdbId = tmdbId;
        return this;
    }

    public MovieBuilder WithTitle(string title)
    {
        _movie.Title = title;
        return this;
    }

    public MovieBuilder WithOverview(string overview)
    {
        _movie.Overview = overview;
        return this;
    }

    public MovieBuilder WithReleaseDate(DateTime releaseDate)
    {
        _movie.ReleaseDate = releaseDate;
        return this;
    }

    public MovieBuilder WithVoteAverage(double voteAverage)
    {
        _movie.VoteAverage = voteAverage;
        return this;
    }

    public MovieBuilder WithVoteCount(int voteCount)
    {
        _movie.VoteCount = voteCount;
        return this;
    }

    public MovieBuilder WithPopularity(double popularity)
    {
        _movie.Popularity = popularity;
        return this;
    }

    public MovieBuilder WithGenres(params string[] genres)
    {
        _movie.Genres = genres;
        return this;
    }

    public MovieBuilder WithPosterPath(string posterPath)
    {
        _movie.PosterPath = posterPath;
        return this;
    }

    public Movie Build() => _movie;
}

