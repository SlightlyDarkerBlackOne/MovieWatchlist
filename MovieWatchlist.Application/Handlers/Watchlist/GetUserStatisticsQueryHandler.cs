using MediatR;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Application.Queries;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class GetMyStatisticsQueryHandler : IRequestHandler<GetMyStatisticsQuery, WatchlistStatistics>
{
    private readonly IWatchlistService _watchlistService;

    public GetMyStatisticsQueryHandler(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    public async Task<WatchlistStatistics> Handle(GetMyStatisticsQuery request, CancellationToken cancellationToken)
    {
        return await _watchlistService.GetUserStatisticsAsync(request);
    }
}
