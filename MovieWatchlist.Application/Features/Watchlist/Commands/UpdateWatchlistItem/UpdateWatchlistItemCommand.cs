using MediatR;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Watchlist.Commands.UpdateWatchlistItem;

public record UpdateWatchlistItemCommand(
    int WatchlistItemId,
    WatchlistStatus? Status = null,
    bool? IsFavorite = null,
    int? UserRating = null,
    string? Notes = null,
    DateTime? WatchedDate = null
) : IRequest<Result<WatchlistItem>>;

