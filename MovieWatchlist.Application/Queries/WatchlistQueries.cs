using MovieWatchlist.Core.Models;
using MediatR;

namespace MovieWatchlist.Application.Queries;

public record GetMyWatchlistQuery() : IRequest<IEnumerable<WatchlistItem>>;

public record GetMyWatchlistByStatusQuery(WatchlistStatus Status) : IRequest<IEnumerable<WatchlistItem>>;

public record GetMyFavoriteMoviesQuery() : IRequest<IEnumerable<WatchlistItem>>;

public record GetMyStatisticsQuery() : IRequest<WatchlistStatistics>;

public record GetMyRecommendedMoviesQuery(int Limit = 10) : IRequest<IEnumerable<Movie>>;

public record GetMyWatchlistByGenreQuery(string Genre) : IRequest<IEnumerable<WatchlistItem>>;

public record GetMyWatchlistByYearRangeQuery(int StartYear, int EndYear) : IRequest<IEnumerable<WatchlistItem>>;

public record GetMyWatchlistByRatingRangeQuery(double MinRating, double MaxRating) : IRequest<IEnumerable<WatchlistItem>>;

public record GetMyWatchlistItemByIdQuery(int WatchlistItemId) : IRequest<WatchlistItem?>;

