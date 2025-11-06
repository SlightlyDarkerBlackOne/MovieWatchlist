using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Common;
using MediatR;

namespace MovieWatchlist.Application.Commands;

public record AddToWatchlistCommand(
    int MovieId,
    WatchlistStatus Status = WatchlistStatus.Planned,
    string? Notes = null
) : IRequest<Result<WatchlistItem>>;

public record UpdateWatchlistItemCommand(
    int WatchlistItemId,
    WatchlistStatus? Status = null,
    bool? IsFavorite = null,
    int? UserRating = null,
    string? Notes = null,
    DateTime? WatchedDate = null
) : IRequest<Result<WatchlistItem>>;

public record RemoveFromWatchlistCommand(
    int WatchlistItemId
) : IRequest<Result<bool>>;

