using System;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.ValueObjects;

namespace MovieWatchlist.Core.Models;

public class WatchlistItem : Entity
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int MovieId { get; private set; }
    public Movie Movie { get; set; } = null!; // Navigation property - kept public for EF Core
    public WatchlistStatus Status { get; private set; } = WatchlistStatus.Planned;
    public bool IsFavorite { get; private set; }
    public Rating? UserRating { get; private set; }
    public string? Notes { get; private set; }
    public DateTime AddedDate { get; private set; } = DateTime.UtcNow;
    public DateTime? WatchedDate { get; private set; }

    /// <summary>
    /// Private parameterless constructor for EF Core
    /// </summary>
    private WatchlistItem() { }

    /// <summary>
    /// Factory method for creating new watchlist items
    /// </summary>
    /// <param name="userId">The ID of the user who owns the watchlist item</param>
    /// <param name="movie">The movie object</param>
    /// <returns>A new watchlist item</returns>
    /// <exception cref="ArgumentException">Thrown when the user ID is not valid</exception>
    /// <exception cref="ArgumentNullException">Thrown when the movie is null</exception>
    public static WatchlistItem Create(int userId, Movie movie)
    {
        if (userId <= 0)
            throw new ArgumentException("User ID must be greater than zero", nameof(userId));
        if (movie == null)
            throw new ArgumentNullException(nameof(movie));

        var item = new WatchlistItem
        {
            UserId = userId,
            Movie = movie,
            Status = WatchlistStatus.Planned,
            AddedDate = DateTime.UtcNow
        };
        
        item.RaiseDomainEvent(new MovieAddedToWatchlistEvent(
            userId,
            movie.TmdbId,
            WatchlistStatus.Planned
        ));
        
        return item;
    }

    // Domain methods for state changes

    /// <summary>
    /// Updates the status of the watchlist item.
    /// Automatically sets WatchedDate when status becomes Watched.
    /// </summary>
    public void UpdateStatus(WatchlistStatus newStatus)
    {
        Status = newStatus;
        
        // Automatically set WatchedDate when marked as watched
        if (newStatus == WatchlistStatus.Watched && !WatchedDate.HasValue)
        {
            WatchedDate = DateTime.UtcNow;
        }
        
        RaiseDomainEvent(new StatisticsInvalidatedEvent(UserId));
    }

    /// <summary>
    /// Convenience method to mark the item as watched.
    /// Idempotent - can be called multiple times safely.
    /// </summary>
    public void MarkAsWatched()
    {
        if (Status != WatchlistStatus.Watched)
        {
            UpdateStatus(WatchlistStatus.Watched);
            RaiseDomainEvent(new MovieWatchedEvent(
                UserId, 
                Movie.TmdbId,
                WatchedDate!.Value
            ));
        }
    }

    /// <summary>
    /// Sets the user's rating for this movie.
    /// Validates that rating is between 1 and 10.
    /// </summary>
    public void SetRating(Rating rating)
    {
        if (rating == null)
            throw new ArgumentNullException(nameof(rating));

        var previousRating = UserRating;
        UserRating = rating;
        
        RaiseDomainEvent(new MovieRatedEvent(
            UserId,
            Movie.TmdbId,
            rating.Value,
            previousRating?.Value
        ));
        
        RaiseDomainEvent(new StatisticsInvalidatedEvent(UserId));
    }

    /// <summary>
    /// Updates the notes for this watchlist item.
    /// </summary>
    public void UpdateNotes(string? notes)
    {
        Notes = notes;
    }

    /// <summary>
    /// Toggles the favorite status of this watchlist item.
    /// </summary>
    public void ToggleFavorite()
    {
        IsFavorite = !IsFavorite;
    }

    /// <summary>
    /// Sets the favorite status explicitly.
    /// </summary>
    public void SetFavorite(bool isFavorite)
    {
        if (IsFavorite != isFavorite)
        {
            IsFavorite = isFavorite;
            RaiseDomainEvent(new MovieFavoritedEvent(
                UserId,
                Movie.TmdbId,
                isFavorite
            ));
            
            RaiseDomainEvent(new StatisticsInvalidatedEvent(UserId));
        }
    }

    /// <summary>
    /// Marks the watchlist item for removal.
    /// Raises the MovieRemovedFromWatchlistEvent before deletion.
    /// </summary>
    public void MarkForRemoval()
    {
        RaiseDomainEvent(new MovieRemovedFromWatchlistEvent(
            UserId,
            Movie.TmdbId,
            Status
        ));
    }

}

public enum WatchlistStatus
{
    Planned,
    Watching,
    Watched,
    Dropped
} 