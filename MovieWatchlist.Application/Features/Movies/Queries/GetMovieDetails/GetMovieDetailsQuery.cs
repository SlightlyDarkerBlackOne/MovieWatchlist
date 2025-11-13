using MediatR;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetails;

public record GetMovieDetailsQuery(int TmdbId) : IRequest<Result<MovieDetailsDto>>;

