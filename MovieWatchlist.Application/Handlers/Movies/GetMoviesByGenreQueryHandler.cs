using MediatR;
using MovieWatchlist.Application.Queries;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Handlers.Movies;

public class GetMoviesByGenreQueryHandler : IRequestHandler<GetMoviesByGenreQuery, Result<IEnumerable<Movie>>>
{
    private readonly ITmdbService _tmdbService;

    public GetMoviesByGenreQueryHandler(ITmdbService tmdbService)
    {
        _tmdbService = tmdbService;
    }

    public async Task<Result<IEnumerable<Movie>>> Handle(GetMoviesByGenreQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var genre = request.Genre?.Trim();
            if (string.IsNullOrWhiteSpace(genre))
                return Result<IEnumerable<Movie>>.Failure("Genre is required.");

            var movies = await _tmdbService.GetMoviesByGenreAsync(genre, request.Page);
            return Result<IEnumerable<Movie>>.Success(movies);
        }
        catch (ArgumentException ex)
        {
            return Result<IEnumerable<Movie>>.Failure(ex.Message);
        }
    }
}

