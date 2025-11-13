using Mapster;
using MediatR;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetailsByTmdbId;

public class GetMovieDetailsByTmdbIdQueryHandler : IRequestHandler<GetMovieDetailsByTmdbIdQuery, Result<MovieDetailsDto>>
{
    private readonly ITmdbService _tmdbService;
    private readonly IMovieRepository _movieRepository;

    public GetMovieDetailsByTmdbIdQueryHandler(
        ITmdbService tmdbService,
        IMovieRepository movieRepository)
    {
        _tmdbService = tmdbService;
        _movieRepository = movieRepository;
    }

    public async Task<Result<MovieDetailsDto>> Handle(GetMovieDetailsByTmdbIdQuery request, CancellationToken cancellationToken)
    {
        var cachedMovie = await _movieRepository.GetByTmdbIdAsync(request.TmdbId);
        if (cachedMovie != null && HasSupplementalData(cachedMovie))
        {
            var dto = cachedMovie.Adapt<MovieDetailsDto>();
            return Result<MovieDetailsDto>.Success(dto);
        }

        var movie = await _tmdbService.GetMovieDetailsAsync(request.TmdbId);
        if (movie == null)
        {
            var notFoundMessage = string.Format(ErrorMessages.MovieWithTmdbIdNotFound, request.TmdbId);
            return Result<MovieDetailsDto>.Failure(notFoundMessage);
        }

        Movie persistedMovie;
        if (cachedMovie == null)
        {
            await _movieRepository.AddAsync(movie);
            persistedMovie = movie;
        }
        else
        {
            UpdateCachedMovie(cachedMovie, movie);
            await _movieRepository.UpdateAsync(cachedMovie);
            persistedMovie = cachedMovie;
        }

        var resultDto = persistedMovie.Adapt<MovieDetailsDto>();
        return Result<MovieDetailsDto>.Success(resultDto);
    }

    private static bool HasSupplementalData(Movie movie)
    {
        return !string.IsNullOrWhiteSpace(movie.CreditsJson) && !string.IsNullOrWhiteSpace(movie.VideosJson);
    }

    private static void UpdateCachedMovie(Movie cachedMovie, Movie freshMovie)
    {
        cachedMovie.Title = freshMovie.Title;
        cachedMovie.Overview = freshMovie.Overview;
        cachedMovie.PosterPath = freshMovie.PosterPath;
        cachedMovie.BackdropPath = freshMovie.BackdropPath;
        cachedMovie.ReleaseDate = freshMovie.ReleaseDate;
        cachedMovie.VoteAverage = freshMovie.VoteAverage;
        cachedMovie.VoteCount = freshMovie.VoteCount;
        cachedMovie.Popularity = freshMovie.Popularity;
        cachedMovie.Genres = freshMovie.Genres;
        cachedMovie.CreditsJson = freshMovie.CreditsJson;
        cachedMovie.VideosJson = freshMovie.VideosJson;
        cachedMovie.UpdatedAt = DateTime.UtcNow;
    }
}

