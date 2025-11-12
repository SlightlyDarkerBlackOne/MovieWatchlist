using MediatR;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Watchlist.Commands.AddToWatchlist;

public record AddToWatchlistCommand(
    int MovieId,
    WatchlistStatus Status = WatchlistStatus.Planned,
    string? Notes = null
) : IRequest<Result<WatchlistItem>>;

