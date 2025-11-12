using MediatR;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByYearRange;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByYearRange;

public class GetMyWatchlistByYearRangeQueryHandler : IRequestHandler<GetMyWatchlistByYearRangeQuery, IEnumerable<WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;

    public GetMyWatchlistByYearRangeQueryHandler(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    public async Task<IEnumerable<WatchlistItem>> Handle(GetMyWatchlistByYearRangeQuery request, CancellationToken cancellationToken)
    {
        return await _watchlistService.GetWatchlistByYearRangeAsync(request);
    }
}

