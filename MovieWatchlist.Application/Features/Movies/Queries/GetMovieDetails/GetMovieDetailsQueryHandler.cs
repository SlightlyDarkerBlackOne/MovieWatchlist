using Mapster;
using MediatR;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetails;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetails;

public class GetMovieDetailsQueryHandler : IRequestHandler<GetMovieDetailsQuery, Result<MovieDetailsDto>>
{
    private readonly ITmdbService _tmdbService;

    public GetMovieDetailsQueryHandler(ITmdbService tmdbService)
    {
        _tmdbService = tmdbService;
    }

    public async Task<Result<MovieDetailsDto>> Handle(GetMovieDetailsQuery request, CancellationToken cancellationToken)
    {
        var movie = await _tmdbService.GetMovieDetailsAsync(request.TmdbId);
        if (movie == null)
            return Result<MovieDetailsDto>.Failure("Movie not found.");

        var dto = movie.Adapt<MovieDetailsDto>();
        return Result<MovieDetailsDto>.Success(dto);
    }
}

