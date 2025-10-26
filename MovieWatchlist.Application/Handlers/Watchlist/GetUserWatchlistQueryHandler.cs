using MediatR;
using MovieWatchlist.Core.Queries;
using MovieWatchlist.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class GetUserWatchlistQueryHandler : IRequestHandler<GetUserWatchlistQuery, IEnumerable<Core.Models.WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;
    private readonly ILogger<GetUserWatchlistQueryHandler> _logger;

    public GetUserWatchlistQueryHandler(
        IWatchlistService watchlistService,
        ILogger<GetUserWatchlistQueryHandler> logger)
    {
        _watchlistService = watchlistService;
        _logger = logger;
    }

    public async Task<IEnumerable<Core.Models.WatchlistItem>> Handle(GetUserWatchlistQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetUserWatchlistQuery for UserId: {UserId}", request.UserId);
        
        var watchlist = await _watchlistService.GetUserWatchlistAsync(request);
        
        _logger.LogInformation("Retrieved {Count} items for UserId: {UserId}", watchlist.Count(), request.UserId);
        
        return watchlist;
    }
}
