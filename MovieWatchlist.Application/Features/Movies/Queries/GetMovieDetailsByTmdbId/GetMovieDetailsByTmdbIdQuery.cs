using MediatR;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetailsByTmdbId;

public record GetMovieDetailsByTmdbIdQuery(int TmdbId) : IRequest<Result<MovieDetailsDto>>;

