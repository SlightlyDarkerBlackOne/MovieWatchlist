using MediatR;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Queries;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class GetRecommendedMoviesQueryHandler : IRequestHandler<GetRecommendedMoviesQuery, IEnumerable<Movie>>
{
    private readonly IWatchlistService _watchlistService;
    private readonly ILogger<GetRecommendedMoviesQueryHandler> _logger;

    public GetRecommendedMoviesQueryHandler(IWatchlistService watchlistService, ILogger<GetRecommendedMoviesQueryHandler> logger)
    {
        _watchlistService = watchlistService;
        _logger = logger;
    }

    public async Task<IEnumerable<Movie>> Handle(GetRecommendedMoviesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling GetRecommendedMoviesQuery for UserId: {UserId}, Limit: {Limit}", request.UserId, request.Limit);
        var recommendations = await _watchlistService.GetRecommendedMoviesAsync(request);
        _logger.LogInformation("Retrieved {Count} recommendations for UserId: {UserId}", recommendations.Count(), request.UserId);
        return recommendations;
    }
}
