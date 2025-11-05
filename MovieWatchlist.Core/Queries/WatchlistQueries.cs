using MovieWatchlist.Core.Models;
using MediatR;

namespace MovieWatchlist.Core.Queries;

public record GetMyWatchlistQuery() : IRequest<IEnumerable<WatchlistItem>>;

public record GetMyWatchlistByStatusQuery(WatchlistStatus Status) : IRequest<IEnumerable<WatchlistItem>>;

public record GetMyFavoriteMoviesQuery() : IRequest<IEnumerable<WatchlistItem>>;

public record GetMyStatisticsQuery() : IRequest<WatchlistStatistics>;

public record GetMyRecommendedMoviesQuery(int Limit = 10) : IRequest<IEnumerable<Movie>>;

public record GetMyWatchlistByGenreQuery(string Genre) : IRequest<IEnumerable<WatchlistItem>>;

public record GetMyWatchlistByYearRangeQuery(int StartYear, int EndYear) : IRequest<IEnumerable<WatchlistItem>>;

public record GetMyWatchlistByRatingRangeQuery(double MinRating, double MaxRating) : IRequest<IEnumerable<WatchlistItem>>;

public record GetMyWatchlistItemByIdQuery(int WatchlistItemId) : IRequest<WatchlistItem?>;

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


