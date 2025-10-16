using System;

namespace MovieWatchlist.Core.Models;

public class WatchlistItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public WatchlistStatus Status { get; set; } = WatchlistStatus.Planned;
    public bool IsFavorite { get; set; }
    public int? UserRating { get; set; }
    public string? Notes { get; set; }
    public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    public DateTime? WatchedDate { get; set; }
}

public enum WatchlistStatus
{
    Planned,
    Watching,
    Watched,
    Dropped
} 