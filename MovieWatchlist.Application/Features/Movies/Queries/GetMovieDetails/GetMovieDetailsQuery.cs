using MediatR;
using MovieWatchlist.Core.Common;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Movies.Queries.GetMovieDetails;

public record GetMovieDetailsQuery(int TmdbId) : IRequest<Result<Movie>>;

