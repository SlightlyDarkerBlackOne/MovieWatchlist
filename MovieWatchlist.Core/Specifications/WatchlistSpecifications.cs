using System;
using System.Linq.Expressions;
using MovieWatchlist.Core.Constants;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Core.Specifications;

public class WatchlistByUserIdSpecification : Specification<WatchlistItem>
{
    private readonly int m_userId;

    public WatchlistByUserIdSpecification(int userId)
    {
        m_userId = userId;
    }

    public override Expression<Func<WatchlistItem, bool>> ToExpression()
    {
        return w => w.UserId == m_userId;
    }
}

public class WatchlistByStatusSpecification : Specification<WatchlistItem>
{
    private readonly WatchlistStatus m_status;

    public WatchlistByStatusSpecification(WatchlistStatus status)
    {
        m_status = status;
    }

    public override Expression<Func<WatchlistItem, bool>> ToExpression()
    {
        return w => w.Status == m_status;
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
    private readonly int m_minRating;

    public HighRatedMoviesSpecification(int minRating = ValidationConstants.Rating.HighRatingThreshold)
    {
        m_minRating = minRating;
    }

    public override Expression<Func<WatchlistItem, bool>> ToExpression()
    {
        return w => w.UserRating != null && w.UserRating.Value >= m_minRating;
    }
}

public class WatchlistByGenreSpecification : Specification<WatchlistItem>
{
    private readonly string m_genre;

    public WatchlistByGenreSpecification(string genre)
    {
        m_genre = genre;
    }

    public override Expression<Func<WatchlistItem, bool>> ToExpression()
    {
        return w => w.Movie.Genres.Contains(m_genre);
    }
}

public class WatchlistByYearRangeSpecification : Specification<WatchlistItem>
{
    private readonly int m_startYear;
    private readonly int m_endYear;

    public WatchlistByYearRangeSpecification(int startYear, int endYear)
    {
        m_startYear = startYear;
        m_endYear = endYear;
    }

    public override Expression<Func<WatchlistItem, bool>> ToExpression()
    {
        return w => w.Movie.ReleaseDate.Year >= m_startYear && w.Movie.ReleaseDate.Year <= m_endYear;
    }
}

public class WatchlistByTmdbRatingRangeSpecification : Specification<WatchlistItem>
{
    private readonly double m_minRating;
    private readonly double m_maxRating;

    public WatchlistByTmdbRatingRangeSpecification(double minRating, double maxRating)
    {
        m_minRating = minRating;
        m_maxRating = maxRating;
    }

    public override Expression<Func<WatchlistItem, bool>> ToExpression()
    {
        return w => w.Movie.VoteAverage >= m_minRating && w.Movie.VoteAverage <= m_maxRating;
    }
}

public class MoviesNotInWatchlistSpecification : Specification<Movie>
{
    private readonly int[] m_movieIds;

    public MoviesNotInWatchlistSpecification(int[] movieIds)
    {
        m_movieIds = movieIds;
    }

    public override Expression<Func<Movie, bool>> ToExpression()
    {
        return m => !m_movieIds.Contains(m.Id);
    }
}

public class MoviesByGenreSpecification : Specification<Movie>
{
    private readonly string m_genre;

    public MoviesByGenreSpecification(string genre)
    {
        m_genre = genre;
    }

    public override Expression<Func<Movie, bool>> ToExpression()
    {
        return m => m.Genres.Contains(m_genre);
    }
}

public class HighTmdbRatedMoviesSpecification : Specification<Movie>
{
    private readonly double m_minRating;

    public HighTmdbRatedMoviesSpecification(double minRating = ValidationConstants.Recommendation.MinTmdbRating)
    {
        m_minRating = minRating;
    }

    public override Expression<Func<Movie, bool>> ToExpression()
    {
        return m => m.VoteAverage >= m_minRating;
    }
}

public class PopularMoviesSpecification : Specification<Movie>
{
    private readonly int m_minVoteCount;

    public PopularMoviesSpecification(int minVoteCount = ValidationConstants.Recommendation.MinVoteCount)
    {
        m_minVoteCount = minVoteCount;
    }

    public override Expression<Func<Movie, bool>> ToExpression()
    {
        return m => m.VoteCount >= m_minVoteCount;
    }
}

