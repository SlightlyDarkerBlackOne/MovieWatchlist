using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.ValueObjects;
using static MovieWatchlist.Tests.Shared.TestDataBuilders.TestDataBuilder;
using Xunit;

namespace MovieWatchlist.Core.UnitTests.Models;

public class WatchlistItemEventTests
{
    [Fact]
    public void MarkAsWatched_RaisesMovieWatchedEvent()
    {
        // Arrange
        var movie = Movie().Build();
        var item = WatchlistItem().WithMovie(movie).Build();
        item.ClearDomainEvents();
        
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
        var movie = Movie().Build();
        var item = WatchlistItem().WithMovie(movie).Build();
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
        var movie = Movie().Build();
        var item = WatchlistItem().WithMovie(movie).Build();
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
        var movie = Movie().Build();
        var item = WatchlistItem().WithMovie(movie).Build();
        item.ClearDomainEvents();
        
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
        var movie = Movie().Build();
        var item = WatchlistItem().WithMovie(movie).Build();
        item.ClearDomainEvents();
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
        var movie = Movie().Build();
        var item = WatchlistItem().WithMovie(movie).Build();
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
        var movie = Movie().Build();
        var item = WatchlistItem().WithMovie(movie).Build();
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
        var movie = Movie().Build();
        var item = WatchlistItem().WithMovie(movie).Build();
        item.MarkAsWatched();
        item.SetRating(Rating.Create(8).Value!);
        item.SetFavorite(true);
        
        Assert.Equal(7, item.DomainEvents.Count);
        
        // Act
        item.ClearDomainEvents();
        
        // Assert
        Assert.Empty(item.DomainEvents);
    }
    
    [Fact]
    public void MultipleOperations_RaisesMultipleEvents()
    {
        // Arrange
        var movie = Movie().Build();
        var item = WatchlistItem().WithMovie(movie).Build();
        
        // Act
        item.MarkAsWatched();
        item.SetRating(Rating.Create(9).Value!);
        item.SetFavorite(true);
        
        // Assert
        var events = item.DomainEvents.ToList();
        Assert.Equal(7, events.Count);
        Assert.IsType<MovieAddedToWatchlistEvent>(events[0]);
        Assert.IsType<StatisticsInvalidatedEvent>(events[1]);
        Assert.IsType<MovieWatchedEvent>(events[2]);
        Assert.IsType<MovieRatedEvent>(events[3]);
        Assert.IsType<StatisticsInvalidatedEvent>(events[4]);
        Assert.IsType<MovieFavoritedEvent>(events[5]);
        Assert.IsType<StatisticsInvalidatedEvent>(events[6]);
    }
    
    [Fact]
    public void DomainEvent_HasOccurredAtTimestamp()
    {
        // Arrange
        var movie = Movie().Build();
        var item = WatchlistItem().WithMovie(movie).Build();
        item.ClearDomainEvents();
        var beforeEvent = DateTime.UtcNow;
        
        // Act
        item.MarkAsWatched();
        
        // Assert
        var watchedEvent = item.DomainEvents.OfType<MovieWatchedEvent>().First();
        Assert.True(watchedEvent.OccurredAt >= beforeEvent);
        Assert.True(watchedEvent.OccurredAt <= DateTime.UtcNow);
    }
    
    [Fact]
    public void DomainEvent_HasUniqueEventId()
    {
        // Arrange
        var movie = Movie().Build();
        var item = WatchlistItem().WithMovie(movie).Build();
        item.ClearDomainEvents();
        
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

