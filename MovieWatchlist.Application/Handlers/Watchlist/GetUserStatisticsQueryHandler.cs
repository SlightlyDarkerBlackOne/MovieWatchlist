using MediatR;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Queries;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class GetUserStatisticsQueryHandler : IRequestHandler<GetUserStatisticsQuery, WatchlistStatistics>
{
    private readonly IWatchlistService _watchlistService;
    private readonly ILogger<GetUserStatisticsQueryHandler> _logger;

    public GetUserStatisticsQueryHandler(IWatchlistService watchlistService, ILogger<GetUserStatisticsQueryHandler> logger)
    {
        _watchlistService = watchlistService;
        _logger = logger;
    }

    public async Task<WatchlistStatistics> Handle(GetUserStatisticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetUserStatisticsQuery for UserId: {UserId}", request.UserId);
        var statistics = await _watchlistService.GetUserStatisticsAsync(request);
        _logger.LogInformation("Retrieved statistics for UserId: {UserId}", request.UserId);
        return statistics;
    }
}
