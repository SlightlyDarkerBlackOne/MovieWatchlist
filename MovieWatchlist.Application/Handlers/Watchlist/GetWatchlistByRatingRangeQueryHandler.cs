using MediatR;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Application.Queries;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Handlers.Watchlist;

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
