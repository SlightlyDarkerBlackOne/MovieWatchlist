using MediatR;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Application.Queries;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class GetMyWatchlistQueryHandler : IRequestHandler<GetMyWatchlistQuery, IEnumerable<Core.Models.WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;

    public GetMyWatchlistQueryHandler(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    public async Task<IEnumerable<Core.Models.WatchlistItem>> Handle(GetMyWatchlistQuery request, CancellationToken cancellationToken)
    {
        return await _watchlistService.GetUserWatchlistAsync(request);
    }
}
