using MediatR;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByRatingRange;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByRatingRange;

public class GetMyWatchlistByRatingRangeQueryHandler : IRequestHandler<GetMyWatchlistByRatingRangeQuery, IEnumerable<WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;

    public GetMyWatchlistByRatingRangeQueryHandler(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    public async Task<IEnumerable<WatchlistItem>> Handle(GetMyWatchlistByRatingRangeQuery request, CancellationToken cancellationToken)
    {
        return await _watchlistService.GetWatchlistByRatingRangeAsync(request);
    }
}

