using MediatR;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Queries;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class GetWatchlistByGenreQueryHandler : IRequestHandler<GetWatchlistByGenreQuery, IEnumerable<WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;
    private readonly ILogger<GetWatchlistByGenreQueryHandler> _logger;

    public GetWatchlistByGenreQueryHandler(IWatchlistService watchlistService, ILogger<GetWatchlistByGenreQueryHandler> logger)
    {
        _watchlistService = watchlistService;
        _logger = logger;
    }

    public async Task<IEnumerable<WatchlistItem>> Handle(GetWatchlistByGenreQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetWatchlistByGenreQuery for UserId: {UserId}, Genre: {Genre}", request.UserId, request.Genre);
        var watchlist = await _watchlistService.GetWatchlistByGenreAsync(request);
        _logger.LogInformation("Retrieved {Count} items for UserId: {UserId}, Genre: {Genre}", watchlist.Count(), request.UserId, request.Genre);
        return watchlist;
    }
}
