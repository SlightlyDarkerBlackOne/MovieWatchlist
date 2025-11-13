using MediatR;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByYearRange;

public record GetMyWatchlistByYearRangeQuery(int StartYear, int EndYear) : IRequest<IEnumerable<WatchlistItem>>;

