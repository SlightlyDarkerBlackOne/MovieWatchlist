using MediatR;
using MovieWatchlist.Application.Features.Movies.Common;
using MovieWatchlist.Core.Common;

namespace MovieWatchlist.Application.Features.Movies.Queries.GetMoviesByGenre;

public record GetMoviesByGenreQuery(string Genre, int Page = 1) : IRequest<Result<IEnumerable<MovieDetailsDto>>>;

