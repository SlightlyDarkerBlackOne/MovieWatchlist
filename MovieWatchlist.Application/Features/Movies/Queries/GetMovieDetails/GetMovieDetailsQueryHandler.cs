using MediatR;
using MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetails;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetails;

public class GetMovieDetailsQueryHandler : IRequestHandler<GetMovieDetailsQuery, Result<Movie>>
{
    private readonly ITmdbService _tmdbService;

    public GetMovieDetailsQueryHandler(ITmdbService tmdbService)
    {
        _tmdbService = tmdbService;
    }

    public async Task<Result<Movie>> Handle(GetMovieDetailsQuery request, CancellationToken cancellationToken)
    {
        var movie = await _tmdbService.GetMovieDetailsAsync(request.TmdbId);
        if (movie == null)
            return Result<Movie>.Failure("Movie not found.");

        return Result<Movie>.Success(movie);
    }
}

