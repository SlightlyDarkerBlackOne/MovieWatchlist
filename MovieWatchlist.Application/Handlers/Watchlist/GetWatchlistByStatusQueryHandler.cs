using MediatR;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Queries;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class GetWatchlistByStatusQueryHandler : IRequestHandler<GetWatchlistByStatusQuery, IEnumerable<WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;
    private readonly ILogger<GetWatchlistByStatusQueryHandler> _logger;

    public GetWatchlistByStatusQueryHandler(IWatchlistService watchlistService, ILogger<GetWatchlistByStatusQueryHandler> logger)
    {
        _watchlistService = watchlistService;
        _logger = logger;
    }

    public async Task<IEnumerable<WatchlistItem>> Handle(GetWatchlistByStatusQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetWatchlistByStatusQuery for UserId: {UserId}, Status: {Status}", request.UserId, request.Status);
        var watchlist = await _watchlistService.GetWatchlistByStatusAsync(request);
        _logger.LogInformation("Retrieved {Count} items for UserId: {UserId}, Status: {Status}", watchlist.Count(), request.UserId, request.Status);
        return watchlist;
    }
}
