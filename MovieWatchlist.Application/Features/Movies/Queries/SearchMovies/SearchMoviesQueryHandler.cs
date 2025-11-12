using MediatR;
using MovieWatchlist.Application.Features.Movies.Queries.SearchMovies;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Movies.Queries.SearchMovies;

public class SearchMoviesQueryHandler : IRequestHandler<SearchMoviesQuery, Result<IEnumerable<Movie>>>
{
    private readonly ITmdbService _tmdbService;

    public SearchMoviesQueryHandler(ITmdbService tmdbService)
    {
        _tmdbService = tmdbService;
    }

    public async Task<Result<IEnumerable<Movie>>> Handle(SearchMoviesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return Result<IEnumerable<Movie>>.Failure(ErrorMessages.SearchQueryRequired);

        var movies = await _tmdbService.SearchMoviesAsync(request.Query, request.Page);
        return Result<IEnumerable<Movie>>.Success(movies);
    }
}

