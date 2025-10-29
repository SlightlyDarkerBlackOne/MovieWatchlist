using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Core.Events;

public record MovieAddedToWatchlistEvent(
    int UserId,
    int MovieId,
    WatchlistStatus InitialStatus
) : DomainEvent;

public record MovieWatchedEvent(
    int UserId,
    int MovieId,
    DateTime WatchedDate
) : DomainEvent;

public record MovieRatedEvent(
    int UserId,
    int MovieId,
    int Rating,
    int? PreviousRating
) : DomainEvent;

public record MovieFavoritedEvent(
    int UserId,
    int MovieId,
    bool IsFavorite
) : DomainEvent;

public record MovieRemovedFromWatchlistEvent(
    int UserId,
    int MovieId,
    WatchlistStatus FinalStatus
) : DomainEvent;

public record StatisticsInvalidatedEvent(
    int UserId
) : DomainEvent;

