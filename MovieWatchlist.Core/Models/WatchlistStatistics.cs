namespace MovieWatchlist.Core.Models;

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

