using Mapster;
using MediatR;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Application.Features.Movies.Queries.GetPopularMovies;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Features.Movies.Queries.GetPopularMovies;

public class GetPopularMoviesQueryHandler : IRequestHandler<GetPopularMoviesQuery, Result<IEnumerable<MovieDetailsDto>>>
{
    private readonly ITmdbService _tmdbService;

    public GetPopularMoviesQueryHandler(ITmdbService tmdbService)
    {
        _tmdbService = tmdbService;
    }

    public async Task<Result<IEnumerable<MovieDetailsDto>>> Handle(GetPopularMoviesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var movies = await _tmdbService.GetPopularMoviesAsync(page);
        var dtos = movies.Adapt<IEnumerable<MovieDetailsDto>>();
        return Result<IEnumerable<MovieDetailsDto>>.Success(dtos);
    }
}

