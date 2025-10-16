using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Tests.Infrastructure;

/// <summary>
/// Fluent builder for creating test data with sensible defaults
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Creates a new User builder with default values
    /// </summary>
    public static UserBuilder User() => new();

    /// <summary>
    /// Creates a new Movie builder with default values
    /// </summary>
    public static MovieBuilder Movie() => new();

    /// <summary>
    /// Creates a new WatchlistItem builder with default values
    /// </summary>
    public static WatchlistItemBuilder WatchlistItem() => new();

    /// <summary>
    /// Creates a new RefreshToken builder with default values
    /// </summary>
    public static RefreshTokenBuilder RefreshToken() => new();
}

/// <summary>
/// Builder for creating User test data
/// </summary>
public class UserBuilder
{
    private readonly User _user;

    public UserBuilder()
    {
        _user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed_password",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            LastLoginAt = DateTime.UtcNow.AddDays(-1)
        };
    }

    public UserBuilder WithId(int id)
    {
        _user.Id = id;
        return this;
    }

    public UserBuilder WithUsername(string username)
    {
        _user.Username = username;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _user.Email = email;
        return this;
    }

    public UserBuilder WithPasswordHash(string passwordHash)
    {
        _user.PasswordHash = passwordHash;
        return this;
    }

    public UserBuilder WithCreatedAt(DateTime createdAt)
    {
        _user.CreatedAt = createdAt;
        return this;
    }

    public UserBuilder WithLastLoginAt(DateTime? lastLoginAt)
    {
        _user.LastLoginAt = lastLoginAt;
        return this;
    }


    public User Build() => _user;
}

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
            TmdbId = 12345,
            Title = "Test Movie",
            Overview = "A test movie for unit testing",
            ReleaseDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            VoteAverage = 7.5,
            VoteCount = 1000,
            Popularity = 50.0,
            PosterPath = "/test-poster.jpg",
            Genres = new[] { "Action", "Drama" }
        };
    }

    public MovieBuilder WithId(int id)
    {
        _movie.Id = id;
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

/// <summary>
/// Builder for creating WatchlistItem test data
/// </summary>
public class WatchlistItemBuilder
{
    private readonly WatchlistItem _item;

    public WatchlistItemBuilder()
    {
        _item = new WatchlistItem
        {
            UserId = 1,
            MovieId = 1,
            Status = WatchlistStatus.Planned,
            IsFavorite = false,
            UserRating = null,
            Notes = null,
            AddedDate = DateTime.UtcNow.AddDays(-7),
            WatchedDate = null
        };
    }

    public WatchlistItemBuilder WithId(int id)
    {
        _item.Id = id;
        return this;
    }

    public WatchlistItemBuilder WithUserId(int userId)
    {
        _item.UserId = userId;
        return this;
    }

    public WatchlistItemBuilder WithMovieId(int movieId)
    {
        _item.MovieId = movieId;
        return this;
    }

    public WatchlistItemBuilder WithStatus(WatchlistStatus status)
    {
        _item.Status = status;
        return this;
    }

    public WatchlistItemBuilder WithIsFavorite(bool isFavorite)
    {
        _item.IsFavorite = isFavorite;
        return this;
    }

    public WatchlistItemBuilder WithUserRating(int? userRating)
    {
        _item.UserRating = userRating;
        return this;
    }

    public WatchlistItemBuilder WithNotes(string? notes)
    {
        _item.Notes = notes;
        return this;
    }

    public WatchlistItemBuilder WithAddedDate(DateTime addedDate)
    {
        _item.AddedDate = addedDate;
        return this;
    }

    public WatchlistItemBuilder WithWatchedDate(DateTime? watchedDate)
    {
        _item.WatchedDate = watchedDate;
        return this;
    }

    public WatchlistItemBuilder WithMovie(Movie movie)
    {
        _item.Movie = movie;
        return this;
    }

    public WatchlistItem Build() => _item;
}

/// <summary>
/// Builder for creating RefreshToken test data
/// </summary>
public class RefreshTokenBuilder
{
    private readonly RefreshToken _token;

    public RefreshTokenBuilder()
    {
        _token = new RefreshToken
        {
            UserId = 1,
            Token = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public RefreshTokenBuilder WithId(int id)
    {
        _token.Id = id;
        return this;
    }

    public RefreshTokenBuilder WithUserId(int userId)
    {
        _token.UserId = userId;
        return this;
    }

    public RefreshTokenBuilder WithToken(string token)
    {
        _token.Token = token;
        return this;
    }

    public RefreshTokenBuilder WithExpiresAt(DateTime expiresAt)
    {
        _token.ExpiresAt = expiresAt;
        return this;
    }

    public RefreshTokenBuilder WithIsRevoked(bool isRevoked)
    {
        _token.IsRevoked = isRevoked;
        return this;
    }

    public RefreshTokenBuilder WithCreatedAt(DateTime createdAt)
    {
        _token.CreatedAt = createdAt;
        return this;
    }

    public RefreshToken Build() => _token;
}
