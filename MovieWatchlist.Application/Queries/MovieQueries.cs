using MediatR;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Queries;

public record SearchMoviesQuery(string Query, int Page = 1) : IRequest<Result<IEnumerable<Movie>>>;

public record GetMovieDetailsQuery(int TmdbId) : IRequest<Result<Movie>>;

public record GetPopularMoviesQuery(int Page = 1) : IRequest<Result<IEnumerable<Movie>>>;

public record GetMoviesByGenreQuery(string Genre, int Page = 1) : IRequest<Result<IEnumerable<Movie>>>;

public record GetMovieDetailsByTmdbIdQuery(int TmdbId) : IRequest<Result<Movie>>;
