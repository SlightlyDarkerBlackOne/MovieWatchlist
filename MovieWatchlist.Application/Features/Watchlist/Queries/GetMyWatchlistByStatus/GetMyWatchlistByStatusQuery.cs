using MediatR;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByStatus;

public record GetMyWatchlistByStatusQuery(WatchlistStatus Status) : IRequest<IEnumerable<WatchlistItem>>;

