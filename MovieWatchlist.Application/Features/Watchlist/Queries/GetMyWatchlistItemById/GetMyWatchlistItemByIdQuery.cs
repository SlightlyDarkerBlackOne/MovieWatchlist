using MediatR;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistItemById;

public record GetMyWatchlistItemByIdQuery(int WatchlistItemId) : IRequest<WatchlistItem?>;

