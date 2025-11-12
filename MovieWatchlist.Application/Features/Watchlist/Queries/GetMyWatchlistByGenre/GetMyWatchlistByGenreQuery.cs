using MediatR;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByGenre;

public record GetMyWatchlistByGenreQuery(string Genre) : IRequest<IEnumerable<WatchlistItem>>;

