namespace MovieWatchlist.Tests.TestDataBuilders;

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

    /// <summary>
    /// Creates a new MovieDetailsDto builder with default values
    /// </summary>
    public static MovieDetailsDtoBuilder MovieDetailsDto() => new();

    /// <summary>
    /// Creates a new TmdbMovieDto builder with default values
    /// </summary>
    public static TmdbMovieDtoBuilder TmdbMovieDto() => new();
}

