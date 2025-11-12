using MediatR;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetailsByTmdbId;

public record GetMovieDetailsByTmdbIdQuery(int TmdbId) : IRequest<Result<Movie>>;

