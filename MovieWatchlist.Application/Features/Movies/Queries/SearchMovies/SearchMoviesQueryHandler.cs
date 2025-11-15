using Mapster;
using MediatR;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Features.Movies.Queries.SearchMovies;

public class SearchMoviesQueryHandler : IRequestHandler<SearchMoviesQuery, Result<IEnumerable<MovieDetailsDto>>>
{
    private readonly ITmdbService _tmdbService;

    public SearchMoviesQueryHandler(ITmdbService tmdbService)
    {
        _tmdbService = tmdbService;
    }

    public async Task<Result<IEnumerable<MovieDetailsDto>>> Handle(SearchMoviesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return Result<IEnumerable<MovieDetailsDto>>.Failure(ErrorMessages.SearchQueryRequired);

        var movies = await _tmdbService.SearchMoviesAsync(request.Query, request.Page);
        var sortedMovies = movies.OrderByDescending(m => m.VoteCount);
        var dtos = sortedMovies.Adapt<IEnumerable<MovieDetailsDto>>();
        return Result<IEnumerable<MovieDetailsDto>>.Success(dtos);
    }
}

