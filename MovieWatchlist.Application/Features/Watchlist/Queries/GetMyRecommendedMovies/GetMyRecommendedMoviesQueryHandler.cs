using MediatR;
using MovieWatchlist.Application.Features.Watchlist.Queries.GetMyRecommendedMovies;
using MovieWatchlist.Application.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Queries.GetMyRecommendedMovies;

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

