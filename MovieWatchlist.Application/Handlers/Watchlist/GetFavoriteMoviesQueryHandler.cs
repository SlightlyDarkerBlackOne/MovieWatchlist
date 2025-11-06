using MediatR;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Application.Queries;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class GetFavoriteMoviesQueryHandler : IRequestHandler<GetMyFavoriteMoviesQuery, IEnumerable<WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;

    public GetFavoriteMoviesQueryHandler(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    public async Task<IEnumerable<WatchlistItem>> Handle(GetMyFavoriteMoviesQuery request, CancellationToken cancellationToken)
    {
        return await _watchlistService.GetFavoriteMoviesAsync(request);
    }
}
