using MediatR;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByGenre;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Queries.GetMyWatchlistByGenre;

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

