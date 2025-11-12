using MediatR;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Movies.Queries.GetMoviesByGenre;

public record GetMoviesByGenreQuery(string Genre, int Page = 1) : IRequest<Result<IEnumerable<Movie>>>;

