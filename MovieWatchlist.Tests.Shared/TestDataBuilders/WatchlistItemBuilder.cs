using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.ValueObjects;

namespace MovieWatchlist.Tests.Shared.TestDataBuilders;

/// <summary>
/// Builder for creating WatchlistItem test data using domain factory methods
/// </summary>
public class WatchlistItemBuilder
{
    private int _userId = 1;
    private WatchlistStatus _status = WatchlistStatus.Planned;
    private bool _isFavorite = false;
    private int? _userRating = null;
    private string? _notes = null;
    private DateTime _addedDate = DateTime.UtcNow;
    private DateTime? _watchedDate = null;
    private Movie? _movie = null;

    public WatchlistItemBuilder WithUserId(int userId)
    {
        _userId = userId;
        return this;
    }

    public WatchlistItemBuilder WithStatus(WatchlistStatus status)
    {
        _status = status;
        return this;
    }

    public WatchlistItemBuilder WithIsFavorite(bool isFavorite)
    {
        _isFavorite = isFavorite;
        return this;
    }

    public WatchlistItemBuilder WithUserRating(int? userRating)
    {
        _userRating = userRating;
        return this;
    }

    public WatchlistItemBuilder WithNotes(string? notes)
    {
        _notes = notes;
        return this;
    }

    public WatchlistItemBuilder WithAddedDate(DateTime addedDate)
    {
        _addedDate = addedDate;
        return this;
    }

    public WatchlistItemBuilder WithWatchedDate(DateTime? watchedDate)
    {
        _watchedDate = watchedDate;
        return this;
    }

    public WatchlistItemBuilder WithMovie(Movie movie)
    {
        _movie = movie;
        return this;
    }

    public WatchlistItem Build()
    {
        var movie = _movie ?? new MovieBuilder().Build();
        var item = WatchlistItem.Create(_userId, movie);

        if (_status != WatchlistStatus.Planned)
            item.UpdateStatus(_status);

        if (_isFavorite)
            item.ToggleFavorite();

        if (_userRating.HasValue)
        {
            var rating = Rating.Create(_userRating.Value).Value!;
            item.SetRating(rating);
        }

        if (_notes != null)
            item.UpdateNotes(_notes);

        return item;
    }
}

