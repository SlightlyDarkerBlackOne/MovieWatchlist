using System;
using System.Linq.Expressions;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Core.Specifications;

public class WatchlistByUserIdSpecification : Specification<WatchlistItem>
{
    private readonly int _userId;

    public WatchlistByUserIdSpecification(int userId)
    {
        _userId = userId;
    }

    public override Expression<Func<WatchlistItem, bool>> ToExpression()
    {
        return w => w.UserId == _userId;
    }
}

public class WatchlistByStatusSpecification : Specification<WatchlistItem>
{
    private readonly WatchlistStatus _status;

    public WatchlistByStatusSpecification(WatchlistStatus status)
    {
        _status = status;
    }

    public override Expression<Func<WatchlistItem, bool>> ToExpression()
    {
        return w => w.Status == _status;
    }
}

public class FavoriteMoviesSpecification : Specification<WatchlistItem>
{
    public override Expression<Func<WatchlistItem, bool>> ToExpression()
    {
        return w => w.IsFavorite == true;
    }
}

public class RatedMoviesSpecification : Specification<WatchlistItem>
{
    public override Expression<Func<WatchlistItem, bool>> ToExpression()
    {
        return w => w.UserRating != null;
    }
}

public class HighRatedMoviesSpecification : Specification<WatchlistItem>
{
    private readonly int _minRating;

    public HighRatedMoviesSpecification(int minRating = ValidationConstants.Rating.HighRatingThreshold)
    {
        _minRating = minRating;
    }

    public override Expression<Func<WatchlistItem, bool>> ToExpression()
    {
        return w => w.UserRating != null && w.UserRating.Value >= _minRating;
    }
}

public class WatchlistByGenreSpecification : Specification<WatchlistItem>
{
    private readonly string _genre;

    public WatchlistByGenreSpecification(string genre)
    {
        _genre = genre;
    }

    public override Expression<Func<WatchlistItem, bool>> ToExpression()
    {
        return w => w.Movie.Genres.Contains(_genre);
    }
}

public class WatchlistByYearRangeSpecification : Specification<WatchlistItem>
{
    private readonly int _startYear;
    private readonly int _endYear;

    public WatchlistByYearRangeSpecification(int startYear, int endYear)
    {
        _startYear = startYear;
        _endYear = endYear;
    }

    public override Expression<Func<WatchlistItem, bool>> ToExpression()
    {
        return w => w.Movie.ReleaseDate.Year >= _startYear && w.Movie.ReleaseDate.Year <= _endYear;
    }
}

public class WatchlistByTmdbRatingRangeSpecification : Specification<WatchlistItem>
{
    private readonly double _minRating;
    private readonly double _maxRating;

    public WatchlistByTmdbRatingRangeSpecification(double minRating, double maxRating)
    {
        _minRating = minRating;
        _maxRating = maxRating;
    }

    public override Expression<Func<WatchlistItem, bool>> ToExpression()
    {
        return w => w.Movie.VoteAverage >= _minRating && w.Movie.VoteAverage <= _maxRating;
    }
}

public class MoviesNotInWatchlistSpecification : Specification<Movie>
{
    private readonly int[] _movieIds;

    public MoviesNotInWatchlistSpecification(int[] movieIds)
    {
        _movieIds = movieIds;
    }

    public override Expression<Func<Movie, bool>> ToExpression()
    {
        return m => !_movieIds.Contains(m.Id);
    }
}

public class MoviesByGenreSpecification : Specification<Movie>
{
    private readonly string _genre;

    public MoviesByGenreSpecification(string genre)
    {
        _genre = genre;
    }

    public override Expression<Func<Movie, bool>> ToExpression()
    {
        return m => m.Genres.Contains(_genre);
    }
}

public class HighTmdbRatedMoviesSpecification : Specification<Movie>
{
    private readonly double _minRating;

    public HighTmdbRatedMoviesSpecification(double minRating = ValidationConstants.Recommendation.MinTmdbRating)
    {
        _minRating = minRating;
    }

    public override Expression<Func<Movie, bool>> ToExpression()
    {
        return m => m.VoteAverage >= _minRating;
    }
}

public class PopularMoviesSpecification : Specification<Movie>
{
    private readonly int _minVoteCount;

    public PopularMoviesSpecification(int minVoteCount = ValidationConstants.Recommendation.MinVoteCount)
    {
        _minVoteCount = minVoteCount;
    }

    public override Expression<Func<Movie, bool>> ToExpression()
    {
        return m => m.VoteCount >= _minVoteCount;
    }
}

