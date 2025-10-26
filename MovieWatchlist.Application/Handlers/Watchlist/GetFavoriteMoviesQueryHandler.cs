using MediatR;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Queries;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class GetFavoriteMoviesQueryHandler : IRequestHandler<GetFavoriteMoviesQuery, IEnumerable<WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;
    private readonly ILogger<GetFavoriteMoviesQueryHandler> _logger;

    public GetFavoriteMoviesQueryHandler(IWatchlistService watchlistService, ILogger<GetFavoriteMoviesQueryHandler> logger)
    {
        _watchlistService = watchlistService;
        _logger = logger;
    }

    public async Task<IEnumerable<WatchlistItem>> Handle(GetFavoriteMoviesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetFavoriteMoviesQuery for UserId: {UserId}", request.UserId);
        var favorites = await _watchlistService.GetFavoriteMoviesAsync(request);
        _logger.LogInformation("Retrieved {Count} favorite items for UserId: {UserId}", favorites.Count(), request.UserId);
        return favorites;
    }
}
