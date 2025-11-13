using MediatR;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Application.Features.Watchlist.Queries.GetMyRecommendedMovies;

public record GetMyRecommendedMoviesQuery(int Limit = 10) : IRequest<IEnumerable<Movie>>;

