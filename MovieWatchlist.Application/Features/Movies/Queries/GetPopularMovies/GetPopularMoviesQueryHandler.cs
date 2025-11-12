using MediatR;
using MovieWatchlist.Application.Features.Movies.Queries.GetPopularMovies;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Movies.Queries.GetPopularMovies;

public class GetPopularMoviesQueryHandler : IRequestHandler<GetPopularMoviesQuery, Result<IEnumerable<Movie>>>
{
    private readonly ITmdbService _tmdbService;

    public GetPopularMoviesQueryHandler(ITmdbService tmdbService)
    {
        _tmdbService = tmdbService;
    }

    public async Task<Result<IEnumerable<Movie>>> Handle(GetPopularMoviesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var movies = await _tmdbService.GetPopularMoviesAsync(page);
        return Result<IEnumerable<Movie>>.Success(movies);
    }
}

