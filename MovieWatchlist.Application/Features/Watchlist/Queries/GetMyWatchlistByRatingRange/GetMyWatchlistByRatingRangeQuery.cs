using MediatR;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByRatingRange;

public record GetMyWatchlistByRatingRangeQuery(double MinRating, double MaxRating) : IRequest<IEnumerable<WatchlistItem>>;

