using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Core.Queries;

public record GetUserWatchlistQuery(int UserId);

public record GetWatchlistByStatusQuery(int UserId, WatchlistStatus Status);

public record GetFavoriteMoviesQuery(int UserId);

public record GetUserStatisticsQuery(int UserId);

public record GetRecommendedMoviesQuery(int UserId, int Limit = 10);

public record GetWatchlistByGenreQuery(int UserId, string Genre);

public record GetWatchlistByYearRangeQuery(int UserId, int StartYear, int EndYear);

public record GetWatchlistByRatingRangeQuery(int UserId, double MinRating, double MaxRating);

public record GetWatchlistItemByIdQuery(int UserId, int WatchlistItemId);

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


