using MediatR;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyFavoriteMovies;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Queries.GetMyFavoriteMovies;

public class GetMyFavoriteMoviesQueryHandler : IRequestHandler<GetMyFavoriteMoviesQuery, IEnumerable<WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;

    public GetMyFavoriteMoviesQueryHandler(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    public async Task<IEnumerable<WatchlistItem>> Handle(GetMyFavoriteMoviesQuery request, CancellationToken cancellationToken)
    {
        return await _watchlistService.GetFavoriteMoviesAsync(request);
    }
}

