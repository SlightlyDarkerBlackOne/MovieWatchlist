using MediatR;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Queries;

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
