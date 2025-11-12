using MediatR;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Movies.Queries.GetPopularMovies;

public record GetPopularMoviesQuery(int Page = 1) : IRequest<Result<IEnumerable<Movie>>>;

