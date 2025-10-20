using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.ValueObjects;

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

    /// <summary>
    /// Creates a new PasswordResetToken builder with default values
    /// </summary>
    public static PasswordResetTokenBuilder PasswordResetToken() => new();
}

/// <summary>
/// Builder for creating User test data
/// </summary>
public class UserBuilder
{
    private int _id = 1;
    private string _username = "testuser";
    private string _email = "test@example.com";
    private string _passwordHash = "hashed_password";
    private DateTime? _lastLoginAt = null;

    public UserBuilder WithId(int id)
    {
        _id = id;
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
        // Create Value Objects
        var username = Username.Create(_username).Value!;
        var email = Email.Create(_email).Value!;
        
        // Create user using domain factory
        var user = User.Create(username, email, _passwordHash);
        
        // Set Id using reflection for testing purposes
        typeof(User).GetProperty("Id")!.SetValue(user, _id);
        
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
        {
            var rating = Rating.Create(_userRating.Value).Value!;
            item.SetRating(rating);
        }

        if (_notes != null)
            item.UpdateNotes(_notes);

        // Clear the navigation property to avoid tracking conflicts
        // The MovieId foreign key is already set correctly
        item.Movie = null!;

        return item;
    }
}

/// <summary>
/// Builder for creating RefreshToken test data using domain factory methods
/// </summary>
public class RefreshTokenBuilder
{
    private int _id = 1;
    private int _userId = 1;
    private string _token = Guid.NewGuid().ToString();
    private int _expirationDays = 7;
    private bool _isRevoked = false;
    private DateTime? _createdAt = null;

    public RefreshTokenBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public RefreshTokenBuilder WithUserId(int userId)
    {
        _userId = userId;
        return this;
    }

    public RefreshTokenBuilder WithToken(string token)
    {
        _token = token;
        return this;
    }

    public RefreshTokenBuilder WithExpiresAt(DateTime expiresAt)
    {
        var daysUntilExpiration = (expiresAt - DateTime.UtcNow).Days;
        _expirationDays = Math.Max(1, daysUntilExpiration);
        return this;
    }

    public RefreshTokenBuilder WithIsRevoked(bool isRevoked)
    {
        _isRevoked = isRevoked;
        return this;
    }

    public RefreshTokenBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public RefreshToken Build()
    {
        var token = RefreshToken.Create(_userId, _token, _expirationDays);

        // Set CreatedAt using reflection if provided
        if (_createdAt.HasValue)
        {
            typeof(RefreshToken).GetProperty("CreatedAt")!.SetValue(token, _createdAt.Value);
        }

        if (_isRevoked)
        {
            token.Revoke();
        }

        return token;
    }
}

/// <summary>
/// Builder for creating PasswordResetToken test data using domain factory methods
/// </summary>
public class PasswordResetTokenBuilder
{
    private int _id = 1;
    private int _userId = 1;
    private string _token = Guid.NewGuid().ToString();
    private int _expirationHours = 1;
    private bool _isUsed = false;
    private DateTime? _createdAt = null;

    public PasswordResetTokenBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public PasswordResetTokenBuilder WithUserId(int userId)
    {
        _userId = userId;
        return this;
    }

    public PasswordResetTokenBuilder WithToken(string token)
    {
        _token = token;
        return this;
    }

    public PasswordResetTokenBuilder WithExpiresAt(DateTime expiresAt)
    {
        // Calculate expiration hours from the provided date
        var hoursUntilExpiration = (int)(expiresAt - DateTime.UtcNow).TotalHours;
        _expirationHours = Math.Max(1, hoursUntilExpiration);
        return this;
    }

    public PasswordResetTokenBuilder WithIsUsed(bool isUsed)
    {
        _isUsed = isUsed;
        return this;
    }

    public PasswordResetTokenBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public PasswordResetToken Build()
    {
        // Create token using domain factory
        var token = PasswordResetToken.Create(_userId, _token, _expirationHours);

        // Set Id using reflection for testing purposes
        typeof(PasswordResetToken).GetProperty("Id")!.SetValue(token, _id);

        // Set CreatedAt using reflection if provided
        if (_createdAt.HasValue)
        {
            typeof(PasswordResetToken).GetProperty("CreatedAt")!.SetValue(token, _createdAt.Value);
        }

        // Mark as used if needed using domain method
        if (_isUsed)
        {
            token.MarkAsUsed();
        }

        return token;
    }
}
