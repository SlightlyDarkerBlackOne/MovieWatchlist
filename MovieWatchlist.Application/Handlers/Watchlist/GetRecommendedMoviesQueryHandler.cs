using MediatR;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Application.Queries;
using MovieWatchlist.Core.Models;

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
