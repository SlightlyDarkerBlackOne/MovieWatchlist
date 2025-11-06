using MediatR;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Application.Queries;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class GetMyWatchlistItemByIdQueryHandler : IRequestHandler<GetMyWatchlistItemByIdQuery, Core.Models.WatchlistItem?>
{
    private readonly IWatchlistService _watchlistService;

    public GetMyWatchlistItemByIdQueryHandler(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    public async Task<Core.Models.WatchlistItem?> Handle(GetMyWatchlistItemByIdQuery request, CancellationToken cancellationToken)
    {
        return await _watchlistService.GetWatchlistItemByIdAsync(request);
    }
}
