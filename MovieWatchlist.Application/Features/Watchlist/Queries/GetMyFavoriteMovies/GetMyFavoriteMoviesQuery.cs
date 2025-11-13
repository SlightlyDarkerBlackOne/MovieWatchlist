using MediatR;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Queries.GetMyFavoriteMovies;

public record GetMyFavoriteMoviesQuery() : IRequest<IEnumerable<WatchlistItem>>;

