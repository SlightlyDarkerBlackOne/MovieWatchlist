using MediatR;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Application.Queries;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class GetMyWatchlistByGenreQueryHandler : IRequestHandler<GetMyWatchlistByGenreQuery, IEnumerable<WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;

    public GetMyWatchlistByGenreQueryHandler(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    public async Task<IEnumerable<WatchlistItem>> Handle(GetMyWatchlistByGenreQuery request, CancellationToken cancellationToken)
    {
        return await _watchlistService.GetWatchlistByGenreAsync(request);
    }
}
