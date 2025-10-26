using MediatR;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Queries;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class GetWatchlistByYearRangeQueryHandler : IRequestHandler<GetWatchlistByYearRangeQuery, IEnumerable<WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;
    private readonly ILogger<GetWatchlistByYearRangeQueryHandler> _logger;

    public GetWatchlistByYearRangeQueryHandler(IWatchlistService watchlistService, ILogger<GetWatchlistByYearRangeQueryHandler> logger)
    {
        _watchlistService = watchlistService;
        _logger = logger;
    }

    public async Task<IEnumerable<WatchlistItem>> Handle(GetWatchlistByYearRangeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetWatchlistByYearRangeQuery for UserId: {UserId}, StartYear: {StartYear}, EndYear: {EndYear}", 
            request.UserId, request.StartYear, request.EndYear);
        var watchlist = await _watchlistService.GetWatchlistByYearRangeAsync(request);
        _logger.LogInformation("Retrieved {Count} items for UserId: {UserId}, YearRange: {StartYear}-{EndYear}", 
            watchlist.Count(), request.UserId, request.StartYear, request.EndYear);
        return watchlist;
    }
}
