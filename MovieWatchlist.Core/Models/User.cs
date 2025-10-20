using System;
using System.Collections.Generic;
using System.Linq;
using MovieWatchlist.Core.Queries;
using MovieWatchlist.Core.ValueObjects;

namespace MovieWatchlist.Core.Models;

public class User : Entity
{
    public int Id { get; private set; }
    public Username Username { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; private set; }
    public ICollection<WatchlistItem> WatchlistItems { get; set; } = new List<WatchlistItem>(); // Navigation property - kept public for EF Core
    public string? CachedStatisticsJson { get; private set; }
    public DateTime? StatisticsLastUpdated { get; private set; }

    private User() { }

    public static User Create(Username username, Email email, string passwordHash)
    {
        if (username == null)
            throw new ArgumentNullException(nameof(username));
        if (email == null)
            throw new ArgumentNullException(nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be null or empty", nameof(passwordHash));

        return new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates the last login timestamp to the current UTC time.
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Changes the user's password hash.
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be null or empty", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
    }

    /// <summary>
    /// Updates the user's email address.
    /// Note: Validation should be performed in the service layer before calling this method.
    /// </summary>
    public void UpdateEmail(Email newEmail)
    {
        if (newEmail == null)
            throw new ArgumentNullException(nameof(newEmail));

        Email = newEmail;
    }

    /// <summary>
    /// Calculates watchlist statistics for the user's watchlist items.
    /// This is domain logic that belongs in the aggregate root.
    /// </summary>
    public WatchlistStatistics CalculateStatistics(IEnumerable<WatchlistItem> watchlistItems)
    {
        if (watchlistItems == null)
            throw new ArgumentNullException(nameof(watchlistItems));

        var totalMovies = 0;
        var watchedMovies = 0;
        var plannedMovies = 0;
        var favoriteMovies = 0;
        var moviesThisYear = 0;
        var ratedMoviesSum = 0.0;
        var ratedMoviesCount = 0;
        var tmdbRatingSum = 0.0;
        var genreCounts = new Dictionary<string, int>();
        var yearlyCounts = new Dictionary<int, int>();
        
        var currentYear = DateTime.UtcNow.Year;
        
        foreach (var item in watchlistItems)
        {
            totalMovies++;
            
            if (item.Status == WatchlistStatus.Watched) watchedMovies++;
            if (item.Status == WatchlistStatus.Planned) plannedMovies++;
            if (item.IsFavorite) favoriteMovies++;
            if (item.AddedDate.Year == currentYear) moviesThisYear++;
            
            if (item.UserRating != null)
            {
                ratedMoviesSum += item.UserRating.Value;
                ratedMoviesCount++;
            }
            
            tmdbRatingSum += item.Movie.VoteAverage;
            
            if (item.Status == WatchlistStatus.Watched)
            {
                foreach (var genre in item.Movie.Genres)
                {
                    genreCounts[genre] = genreCounts.GetValueOrDefault(genre, 0) + 1;
                }
                
                var year = item.Movie.ReleaseDate.Year;
                yearlyCounts[year] = yearlyCounts.GetValueOrDefault(year, 0) + 1;
            }
        }
        
        var averageUserRating = ratedMoviesCount > 0 ? ratedMoviesSum / ratedMoviesCount : 0;
        var averageTmdbRating = totalMovies > 0 ? tmdbRatingSum / totalMovies : 0;
        
        var mostWatchedGenre = genreCounts
            .OrderByDescending(kvp => kvp.Value)
            .FirstOrDefault().Key ?? string.Empty;
        
        var yearlyBreakdown = yearlyCounts
            .OrderByDescending(kvp => kvp.Key)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        return new WatchlistStatistics(
            TotalMovies: totalMovies,
            WatchedMovies: watchedMovies,
            PlannedMovies: plannedMovies,
            FavoriteMovies: favoriteMovies,
            AverageUserRating: averageUserRating,
            AverageTmdbRating: averageTmdbRating,
            MostWatchedGenre: mostWatchedGenre,
            MoviesThisYear: moviesThisYear,
            GenreBreakdown: genreCounts,
            YearlyBreakdown: yearlyBreakdown
        );
    }

    public void UpdateCachedStatistics(WatchlistStatistics statistics)
    {
        CachedStatisticsJson = System.Text.Json.JsonSerializer.Serialize(statistics);
        StatisticsLastUpdated = DateTime.UtcNow;
    }

    public WatchlistStatistics? GetCachedStatistics()
    {
        if (string.IsNullOrEmpty(CachedStatisticsJson))
            return null;
        
        return System.Text.Json.JsonSerializer.Deserialize<WatchlistStatistics>(CachedStatisticsJson);
    }

    public void InvalidateStatistics()
    {
        CachedStatisticsJson = null;
        StatisticsLastUpdated = null;
    }

    public bool HasValidStatistics() => !string.IsNullOrEmpty(CachedStatisticsJson);
} 