using MediatR;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Queries;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class GetMyWatchlistByStatusQueryHandler : IRequestHandler<GetMyWatchlistByStatusQuery, IEnumerable<WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;

    public GetMyWatchlistByStatusQueryHandler(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    public async Task<IEnumerable<WatchlistItem>> Handle(GetMyWatchlistByStatusQuery request, CancellationToken cancellationToken)
    {
        return await _watchlistService.GetWatchlistByStatusAsync(request);
    }
}
