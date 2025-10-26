using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.ValueObjects;
using MovieWatchlist.Tests.Infrastructure;
using Xunit;

namespace MovieWatchlist.Tests.Core.Models;

public class WatchlistItemEventTests : UnitTestBase
{
    [Fact]
    public void MarkAsWatched_RaisesMovieWatchedEvent()
    {
        // Arrange
        var movie = CreateTestMovie(id: 1, title: "Test Movie");
        var item = WatchlistItem.Create(1, movie);
        
        // Act
        item.MarkAsWatched();
        
        // Assert
        var events = item.DomainEvents.ToList();
        Assert.Equal(2, events.Count);
        Assert.IsType<StatisticsInvalidatedEvent>(events[0]);
        Assert.IsType<MovieWatchedEvent>(events[1]);
        
        var watchedEvent = (MovieWatchedEvent)events[1];
        Assert.Equal(1, watchedEvent.UserId);
        Assert.Equal(12345, watchedEvent.MovieId);
        Assert.NotEqual(default(DateTime), watchedEvent.WatchedDate);
        Assert.NotEqual(Guid.Empty, watchedEvent.EventId);
    }
    
    [Fact]
    public void MarkAsWatched_DoesNotRaiseDuplicateEvents()
    {
        // Arrange
        var movie = CreateTestMovie(id: 1, title: "Test Movie");
        var item = WatchlistItem.Create(1, movie);
        item.MarkAsWatched();
        item.ClearDomainEvents();
        
        // Act
        item.MarkAsWatched(); // Idempotent - should not raise event again
        
        // Assert
        var events = item.DomainEvents.ToList();
        Assert.Empty(events);
    }
    
    [Fact]
    public void SetRating_RaisesMovieRatedEvent_WithPreviousRating()
    {
        // Arrange
        var movie = CreateTestMovie(id: 1, title: "Test Movie");
        var item = WatchlistItem.Create(1, movie);
        item.SetRating(Rating.Create(5).Value!);
        item.ClearDomainEvents();
        
        // Act
        item.SetRating(Rating.Create(8).Value!);
        
        // Assert
        var events = item.DomainEvents.ToList();
        Assert.Equal(2, events.Count);
        Assert.IsType<MovieRatedEvent>(events[0]);
        Assert.IsType<StatisticsInvalidatedEvent>(events[1]);
        
        var ratedEvent = (MovieRatedEvent)events[0];
        Assert.Equal(1, ratedEvent.UserId);
        Assert.Equal(12345, ratedEvent.MovieId);
        Assert.Equal(8, ratedEvent.Rating);
        Assert.Equal(5, ratedEvent.PreviousRating);
    }
    
    [Fact]
    public void SetRating_FirstRating_RaisesEvent_WithNullPreviousRating()
    {
        // Arrange
        var movie = CreateTestMovie(id: 1, title: "Test Movie");
        var item = WatchlistItem.Create(1, movie);
        
        // Act
        item.SetRating(Rating.Create(7).Value!);
        
        // Assert
        var events = item.DomainEvents.ToList();
        Assert.Equal(2, events.Count);
        Assert.IsType<MovieRatedEvent>(events[0]);
        Assert.IsType<StatisticsInvalidatedEvent>(events[1]);
        
        var ratedEvent = (MovieRatedEvent)events[0];
        Assert.Equal(7, ratedEvent.Rating);
        Assert.Null(ratedEvent.PreviousRating);
    }
    
    [Fact]
    public void SetFavorite_True_RaisesMovieFavoritedEvent()
    {
        // Arrange
        var movie = CreateTestMovie(id: 1, title: "Test Movie");
        var item = WatchlistItem.Create(1, movie);
        Assert.False(item.IsFavorite);
        
        // Act
        item.SetFavorite(true);
        
        // Assert
        var events = item.DomainEvents.ToList();
        Assert.Equal(2, events.Count);
        Assert.IsType<MovieFavoritedEvent>(events[0]);
        Assert.IsType<StatisticsInvalidatedEvent>(events[1]);
        
        var favoritedEvent = (MovieFavoritedEvent)events[0];
        Assert.Equal(1, favoritedEvent.UserId);
        Assert.Equal(12345, favoritedEvent.MovieId);
        Assert.True(favoritedEvent.IsFavorite);
        Assert.True(item.IsFavorite);
    }
    
    [Fact]
    public void SetFavorite_False_RaisesMovieFavoritedEvent()
    {
        // Arrange
        var movie = CreateTestMovie(id: 1, title: "Test Movie");
        var item = WatchlistItem.Create(1, movie);
        item.SetFavorite(true);
        item.ClearDomainEvents();
        
        // Act
        item.SetFavorite(false);
        
        // Assert
        var events = item.DomainEvents.ToList();
        Assert.Equal(2, events.Count);
        Assert.IsType<MovieFavoritedEvent>(events[0]);
        Assert.IsType<StatisticsInvalidatedEvent>(events[1]);
        
        var favoritedEvent = (MovieFavoritedEvent)events[0];
        Assert.False(favoritedEvent.IsFavorite);
        Assert.False(item.IsFavorite);
    }
    
    [Fact]
    public void SetFavorite_SameValue_DoesNotRaiseEvent()
    {
        // Arrange
        var movie = CreateTestMovie(id: 1, title: "Test Movie");
        var item = WatchlistItem.Create(1, movie);
        item.SetFavorite(true);
        item.ClearDomainEvents();
        
        // Act
        item.SetFavorite(true); // Same value - should not raise event
        
        // Assert
        var events = item.DomainEvents.ToList();
        Assert.Empty(events);
    }
    
    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        // Arrange
        var movie = CreateTestMovie(id: 1, title: "Test Movie");
        var item = WatchlistItem.Create(1, movie);
        item.MarkAsWatched();
        item.SetRating(Rating.Create(8).Value!);
        item.SetFavorite(true);
        
        Assert.Equal(6, item.DomainEvents.Count);
        
        // Act
        item.ClearDomainEvents();
        
        // Assert
        Assert.Empty(item.DomainEvents);
    }
    
    [Fact]
    public void MultipleOperations_RaisesMultipleEvents()
    {
        // Arrange
        var movie = CreateTestMovie(id: 1, title: "Test Movie");
        var item = WatchlistItem.Create(1, movie);
        
        // Act
        item.MarkAsWatched();
        item.SetRating(Rating.Create(9).Value!);
        item.SetFavorite(true);
        
        // Assert
        var events = item.DomainEvents.ToList();
        Assert.Equal(6, events.Count);
        Assert.IsType<StatisticsInvalidatedEvent>(events[0]);
        Assert.IsType<MovieWatchedEvent>(events[1]);
        Assert.IsType<MovieRatedEvent>(events[2]);
        Assert.IsType<StatisticsInvalidatedEvent>(events[3]);
        Assert.IsType<MovieFavoritedEvent>(events[4]);
        Assert.IsType<StatisticsInvalidatedEvent>(events[5]);
    }
    
    [Fact]
    public void DomainEvent_HasOccurredAtTimestamp()
    {
        // Arrange
        var movie = CreateTestMovie(id: 1, title: "Test Movie");
        var item = WatchlistItem.Create(1, movie);
        var beforeEvent = DateTime.UtcNow;
        
        // Act
        item.MarkAsWatched();
        
        // Assert
        var watchedEvent = (MovieWatchedEvent)item.DomainEvents.Skip(1).First();
        Assert.True(watchedEvent.OccurredAt >= beforeEvent);
        Assert.True(watchedEvent.OccurredAt <= DateTime.UtcNow);
    }
    
    [Fact]
    public void DomainEvent_HasUniqueEventId()
    {
        // Arrange
        var movie = CreateTestMovie(id: 1, title: "Test Movie");
        var item = WatchlistItem.Create(1, movie);
        
        // Act
        item.MarkAsWatched();
        item.SetRating(Rating.Create(7).Value!);
        
        // Assert
        var events = item.DomainEvents.ToList();
        var eventIds = events.Select(e => e.EventId).ToList();
        
        Assert.Equal(4, eventIds.Count);
        Assert.NotEqual(eventIds[0], eventIds[1]);
        Assert.All(eventIds, id => Assert.NotEqual(Guid.Empty, id));
    }
}

