using MovieWatchlist.Core.Models;
using MediatR;

namespace MovieWatchlist.Core.Queries;

public record GetUserWatchlistQuery(int UserId) : IRequest<IEnumerable<WatchlistItem>>;

public record GetWatchlistByStatusQuery(int UserId, WatchlistStatus Status) : IRequest<IEnumerable<WatchlistItem>>;

public record GetFavoriteMoviesQuery(int UserId) : IRequest<IEnumerable<WatchlistItem>>;

public record GetUserStatisticsQuery(int UserId) : IRequest<WatchlistStatistics>;

public record GetRecommendedMoviesQuery(int UserId, int Limit = 10) : IRequest<IEnumerable<Movie>>;

public record GetWatchlistByGenreQuery(int UserId, string Genre) : IRequest<IEnumerable<WatchlistItem>>;

public record GetWatchlistByYearRangeQuery(int UserId, int StartYear, int EndYear) : IRequest<IEnumerable<WatchlistItem>>;

public record GetWatchlistByRatingRangeQuery(int UserId, double MinRating, double MaxRating) : IRequest<IEnumerable<WatchlistItem>>;

public record GetWatchlistItemByIdQuery(int UserId, int WatchlistItemId) : IRequest<WatchlistItem?>;

public record WatchlistStatistics(
    int TotalMovies,
    int WatchedMovies,
    int PlannedMovies,
    int FavoriteMovies,
    double AverageUserRating,
    double AverageTmdbRating,
    string MostWatchedGenre,
    int MoviesThisYear,
    Dictionary<string, int> GenreBreakdown,
    Dictionary<int, int> YearlyBreakdown
);


