using MediatR;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Movies.Queries.GetPopularMovies;

public record GetPopularMoviesQuery(int Page = 1) : IRequest<Result<IEnumerable<MovieDetailsDto>>>;

