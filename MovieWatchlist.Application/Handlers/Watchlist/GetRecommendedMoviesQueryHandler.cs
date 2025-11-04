using MediatR;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Queries;

namespace MovieWatchlist.Application.Handlers.Watchlist;

public class GetMyRecommendedMoviesQueryHandler : IRequestHandler<GetMyRecommendedMoviesQuery, IEnumerable<Movie>>
{
    private readonly IWatchlistService _watchlistService;

    public GetMyRecommendedMoviesQueryHandler(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    public async Task<IEnumerable<Movie>> Handle(GetMyRecommendedMoviesQuery request, CancellationToken cancellationToken)
    {
        return await _watchlistService.GetRecommendedMoviesAsync(request);
    }
}
