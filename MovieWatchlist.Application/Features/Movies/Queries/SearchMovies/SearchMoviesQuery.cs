using MediatR;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Movies.Queries.SearchMovies;

public record SearchMoviesQuery(string Query, int Page = 1) : IRequest<Result<IEnumerable<Movie>>>;

