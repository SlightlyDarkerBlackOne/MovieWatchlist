using MediatR;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlist;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlist;

public class GetMyWatchlistQueryHandler : IRequestHandler<GetMyWatchlistQuery, IEnumerable<WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;

    public GetMyWatchlistQueryHandler(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    public async Task<IEnumerable<WatchlistItem>> Handle(GetMyWatchlistQuery request, CancellationToken cancellationToken)
    {
        return await _watchlistService.GetUserWatchlistAsync(request);
    }
}

