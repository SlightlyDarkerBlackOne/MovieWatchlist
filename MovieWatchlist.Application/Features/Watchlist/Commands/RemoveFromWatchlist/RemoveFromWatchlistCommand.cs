using MediatR;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Watchlist.Commands.RemoveFromWatchlist;

public record RemoveFromWatchlistCommand(
    int WatchlistItemId
) : IRequest<Result<bool>>;

