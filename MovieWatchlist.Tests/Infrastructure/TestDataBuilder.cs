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
    private string _username = "testuser";
    private string _email = "test@example.com";
    private string _passwordHash = "hashed_password";
    private DateTime? _lastLoginAt = null;

    public UserBuilder WithId(int id)
    {
        // Note: Id is set by EF Core, cannot be set via domain methods
        // This is kept for backward compatibility but will be ignored
        return this;
    }

    public UserBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithPasswordHash(string passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    public UserBuilder WithCreatedAt(DateTime createdAt)
    {
        // Note: CreatedAt is set by factory method, cannot be changed via domain methods
        // This is kept for backward compatibility but will be ignored
        return this;
    }

    public UserBuilder WithLastLoginAt(DateTime? lastLoginAt)
    {
        _lastLoginAt = lastLoginAt;
        return this;
    }

    public User Build()
    {
        // Create user using domain factory
        var user = User.Create(_username, _email, _passwordHash);
        
        // Set LastLoginAt using reflection if provided
        if (_lastLoginAt.HasValue)
        {
            typeof(User).GetProperty("LastLoginAt")!.SetValue(user, _lastLoginAt.Value);
        }
        
        return user;
    }
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
/// Builder for creating WatchlistItem test data using domain factory methods
/// </summary>
public class WatchlistItemBuilder
{
    private int _userId = 1;
    private int _movieId = 1;
    private WatchlistStatus _status = WatchlistStatus.Planned;
    private bool _isFavorite = false;
    private int? _userRating = null;
    private string? _notes = null;
    private DateTime _addedDate = DateTime.UtcNow;
    private DateTime? _watchedDate = null;
    private static int _uniqueMovieCounter = 100000; // Start at high number to avoid conflicts

    public WatchlistItemBuilder WithId(int id)
    {
        // Note: Id is set by EF Core, cannot be set via domain methods
        // This is kept for backward compatibility but will be ignored
        return this;
    }

    public WatchlistItemBuilder WithUserId(int userId)
    {
        _userId = userId;
        return this;
    }

    public WatchlistItemBuilder WithMovieId(int movieId)
    {
        _movieId = movieId;
        return this;
    }

    public WatchlistItemBuilder WithStatus(WatchlistStatus status)
    {
        _status = status;
        return this;
    }

    public WatchlistItemBuilder WithIsFavorite(bool isFavorite)
    {
        _isFavorite = isFavorite;
        return this;
    }

    public WatchlistItemBuilder WithUserRating(int? userRating)
    {
        _userRating = userRating;
        return this;
    }

    public WatchlistItemBuilder WithNotes(string? notes)
    {
        _notes = notes;
        return this;
    }

    public WatchlistItemBuilder WithAddedDate(DateTime addedDate)
    {
        _addedDate = addedDate;
        return this;
    }

    public WatchlistItemBuilder WithWatchedDate(DateTime? watchedDate)
    {
        _watchedDate = watchedDate;
        return this;
    }

    public WatchlistItemBuilder WithMovie(Movie movie)
    {
        // Note: Movie parameter is ignored - we only use MovieId for foreign key
        return this;
    }

    public WatchlistItem Build()
    {
        // Create a unique movie object to satisfy the domain factory requirement
        // This movie won't be saved to the database - only the MovieId FK is used
        var uniqueMovie = new Movie
        {
            TmdbId = Interlocked.Increment(ref _uniqueMovieCounter), // Thread-safe unique ID
            Title = $"Test Movie {_movieId}",
            Overview = "A test movie for watchlist item",
            PosterPath = "/test.jpg",
            ReleaseDate = DateTime.UtcNow,
            VoteAverage = 7.5,
            VoteCount = 1000,
            Popularity = 100.0,
            Genres = new[] { "Action", "Drama" }
        };

        // Create watchlist item using domain factory
        var item = WatchlistItem.Create(_userId, _movieId, uniqueMovie);

        // Update properties that weren't set by factory
        if (_status != WatchlistStatus.Planned)
            item.UpdateStatus(_status);

        if (_isFavorite)
            item.ToggleFavorite();

        if (_userRating.HasValue)
            item.SetRating(_userRating.Value);

        if (_notes != null)
            item.UpdateNotes(_notes);

        // Clear the navigation property to avoid tracking conflicts
        // The MovieId foreign key is already set correctly
        item.Movie = null!;

        return item;
    }
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
