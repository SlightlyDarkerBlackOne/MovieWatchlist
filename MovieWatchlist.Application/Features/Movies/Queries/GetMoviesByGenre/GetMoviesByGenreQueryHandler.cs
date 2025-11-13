using Mapster;
using MediatR;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Application.Features.Movies.Queries.GetMoviesByGenre;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;

namespace MovieWatchlist.Application.Features.Movies.Queries.GetMoviesByGenre;

public class GetMoviesByGenreQueryHandler : IRequestHandler<GetMoviesByGenreQuery, Result<IEnumerable<MovieDetailsDto>>>
{
    private readonly ITmdbService _tmdbService;

    public GetMoviesByGenreQueryHandler(ITmdbService tmdbService)
    {
        _tmdbService = tmdbService;
    }

    public async Task<Result<IEnumerable<MovieDetailsDto>>> Handle(GetMoviesByGenreQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var genre = request.Genre?.Trim();
            if (string.IsNullOrWhiteSpace(genre))
                return Result<IEnumerable<MovieDetailsDto>>.Failure("Genre is required.");

            var movies = await _tmdbService.GetMoviesByGenreAsync(genre, request.Page);
            var dtos = movies.Adapt<IEnumerable<MovieDetailsDto>>();
            return Result<IEnumerable<MovieDetailsDto>>.Success(dtos);
        }
        catch (ArgumentException ex)
        {
            return Result<IEnumerable<MovieDetailsDto>>.Failure(ex.Message);
        }
    }
}

