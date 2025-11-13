using MediatR;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlist;

public record GetMyWatchlistQuery() : IRequest<IEnumerable<WatchlistItem>>;

