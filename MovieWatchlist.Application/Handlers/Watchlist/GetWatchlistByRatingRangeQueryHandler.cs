using MediatR;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Queries;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class GetWatchlistByRatingRangeQueryHandler : IRequestHandler<GetWatchlistByRatingRangeQuery, IEnumerable<WatchlistItem>>
{
    private readonly IWatchlistService _watchlistService;
    private readonly ILogger<GetWatchlistByRatingRangeQueryHandler> _logger;

    public GetWatchlistByRatingRangeQueryHandler(IWatchlistService watchlistService, ILogger<GetWatchlistByRatingRangeQueryHandler> logger)
    {
        _watchlistService = watchlistService;
        _logger = logger;
    }

    public async Task<IEnumerable<WatchlistItem>> Handle(GetWatchlistByRatingRangeQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetWatchlistByRatingRangeQuery for UserId: {UserId}, MinRating: {MinRating}, MaxRating: {MaxRating}", 
            request.UserId, request.MinRating, request.MaxRating);
        var watchlist = await _watchlistService.GetWatchlistByRatingRangeAsync(request);
        _logger.LogInformation("Retrieved {Count} items for UserId: {UserId}, RatingRange: {MinRating}-{MaxRating}", 
            watchlist.Count(), request.UserId, request.MinRating, request.MaxRating);
        return watchlist;
    }
}
