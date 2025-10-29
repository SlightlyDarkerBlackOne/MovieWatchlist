using MediatR;
using MovieWatchlist.Core.Queries;
using MovieWatchlist.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class GetWatchlistItemByIdQueryHandler : IRequestHandler<GetWatchlistItemByIdQuery, Core.Models.WatchlistItem?>
{
    private readonly IWatchlistService _watchlistService;
    private readonly ILogger<GetWatchlistItemByIdQueryHandler> _logger;

    public GetWatchlistItemByIdQueryHandler(
        IWatchlistService watchlistService,
        ILogger<GetWatchlistItemByIdQueryHandler> logger)
    {
        _watchlistService = watchlistService;
        _logger = logger;
    }

    public async Task<Core.Models.WatchlistItem?> Handle(GetWatchlistItemByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetWatchlistItemByIdQuery for UserId: {UserId}, WatchlistItemId: {WatchlistItemId}", request.UserId, request.WatchlistItemId);
        
        var watchlistItem = await _watchlistService.GetWatchlistItemByIdAsync(request);
        
        if (watchlistItem == null)
        {
            _logger.LogWarning("Watchlist item not found for UserId: {UserId}, WatchlistItemId: {WatchlistItemId}", request.UserId, request.WatchlistItemId);
        }
        else
        {
            _logger.LogInformation("Retrieved watchlist item for UserId: {UserId}, WatchlistItemId: {WatchlistItemId}", request.UserId, request.WatchlistItemId);
        }
        
        return watchlistItem;
    }
}
