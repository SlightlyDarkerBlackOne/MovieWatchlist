using MediatR;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistItemById;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistItemById;

public class GetMyWatchlistItemByIdQueryHandler : IRequestHandler<GetMyWatchlistItemByIdQuery, WatchlistItem?>
{
    private readonly IWatchlistService _watchlistService;

    public GetMyWatchlistItemByIdQueryHandler(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    public async Task<WatchlistItem?> Handle(GetMyWatchlistItemByIdQuery request, CancellationToken cancellationToken)
    {
        return await _watchlistService.GetWatchlistItemByIdAsync(request);
    }
}

