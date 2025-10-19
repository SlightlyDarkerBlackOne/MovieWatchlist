using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Core.Commands;

public record AddToWatchlistCommand(
    int UserId,
    int MovieId,
    WatchlistStatus Status = WatchlistStatus.Planned,
    string? Notes = null
);

public record UpdateWatchlistItemCommand(
    int UserId,
    int WatchlistItemId,
    WatchlistStatus? Status = null,
    bool? IsFavorite = null,
    int? UserRating = null,
    string? Notes = null,
    DateTime? WatchedDate = null
);

public record RemoveFromWatchlistCommand(
    int UserId,
    int WatchlistItemId
);


