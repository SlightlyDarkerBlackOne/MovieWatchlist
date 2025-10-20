using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MovieWatchlist.Core.Events;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.ValueObjects;
using MovieWatchlist.Infrastructure.Data;
using MovieWatchlist.Tests.Infrastructure;
using Xunit;

namespace MovieWatchlist.Tests.Integration;

public class DomainEventsIntegrationTests : EnhancedIntegrationTestBase
{
    private readonly IUnitOfWork m_unitOfWork;
    
    public DomainEventsIntegrationTests(WebApplicationFactory<Program> factory) 
        : base(factory)
    {
        m_unitOfWork = Scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    }
    
    [Fact]
    public async Task UpdateWatchlistItem_ToWatched_RaisesAndDispatchesEvent()
    {
        // Arrange
        await InitializeDatabaseAsync();
        
        var user = TestDataBuilder.User()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();
        
        var movie = TestDataBuilder.Movie()
            .WithTitle("Inception")
            .WithTmdbId(27205)
            .Build();
        
        Context.Users.Add(user);
        Context.Movies.Add(movie);
        await m_unitOfWork.SaveChangesAsync();
        
        var item = WatchlistItem.Create(user.Id, movie.Id, movie);
        Context.WatchlistItems.Add(item);
        await m_unitOfWork.SaveChangesAsync();
        
        // Act
        item.MarkAsWatched();
        await m_unitOfWork.SaveChangesAsync();
        
        // Assert
        var events = item.DomainEvents.ToList();
        events.Should().BeEmpty(); // Events should be cleared after dispatch
        
        // Verify the state change persisted
        var savedItem = await Context.WatchlistItems
            .FirstOrDefaultAsync(w => w.Id == item.Id);
        
        savedItem.Should().NotBeNull();
        savedItem!.Status.Should().Be(WatchlistStatus.Watched);
        savedItem.WatchedDate.Should().NotBeNull();
        
        await CleanupDatabaseAsync();
    }
    
    [Fact]
    public async Task UpdateWatchlistItem_WithRating_RaisesAndDispatchesEvent()
    {
        // Arrange
        await InitializeDatabaseAsync();
        
        var user = TestDataBuilder.User()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();
        
        var movie = TestDataBuilder.Movie()
            .WithTitle("The Dark Knight")
            .WithTmdbId(155)
            .Build();
        
        Context.Users.Add(user);
        Context.Movies.Add(movie);
        await m_unitOfWork.SaveChangesAsync();
        
        var item = WatchlistItem.Create(user.Id, movie.Id, movie);
        Context.WatchlistItems.Add(item);
        await m_unitOfWork.SaveChangesAsync();
        
        // Act
        item.SetRating(Rating.Create(9).Value!);
        await m_unitOfWork.SaveChangesAsync();
        
        // Assert
        var events = item.DomainEvents.ToList();
        events.Should().BeEmpty(); // Events should be cleared after dispatch
        
        // Verify the state change persisted
        var savedItem = await Context.WatchlistItems
            .FirstOrDefaultAsync(w => w.Id == item.Id);
        
        savedItem.Should().NotBeNull();
        savedItem!.UserRating.Should().NotBeNull();
        savedItem.UserRating!.Value.Should().Be(9);
        
        await CleanupDatabaseAsync();
    }
    
    [Fact]
    public async Task UpdateWatchlistItem_WithFavorite_RaisesAndDispatchesEvent()
    {
        // Arrange
        await InitializeDatabaseAsync();
        
        var user = TestDataBuilder.User()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();
        
        var movie = TestDataBuilder.Movie()
            .WithTitle("Pulp Fiction")
            .WithTmdbId(680)
            .Build();
        
        Context.Users.Add(user);
        Context.Movies.Add(movie);
        await m_unitOfWork.SaveChangesAsync();
        
        var item = WatchlistItem.Create(user.Id, movie.Id, movie);
        Context.WatchlistItems.Add(item);
        await m_unitOfWork.SaveChangesAsync();
        
        // Act
        item.SetFavorite(true);
        await m_unitOfWork.SaveChangesAsync();
        
        // Assert
        var events = item.DomainEvents.ToList();
        events.Should().BeEmpty(); // Events should be cleared after dispatch
        
        // Verify the state change persisted
        var savedItem = await Context.WatchlistItems
            .FirstOrDefaultAsync(w => w.Id == item.Id);
        
        savedItem.Should().NotBeNull();
        savedItem!.IsFavorite.Should().BeTrue();
        
        await CleanupDatabaseAsync();
    }
    
    [Fact]
    public async Task MultipleOperations_OnSameItem_RaisesAndDispatchesMultipleEvents()
    {
        // Arrange
        await InitializeDatabaseAsync();
        
        var user = TestDataBuilder.User()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();
        
        var movie = TestDataBuilder.Movie()
            .WithTitle("The Matrix")
            .WithTmdbId(603)
            .Build();
        
        Context.Users.Add(user);
        Context.Movies.Add(movie);
        await m_unitOfWork.SaveChangesAsync();
        
        var item = WatchlistItem.Create(user.Id, movie.Id, movie);
        Context.WatchlistItems.Add(item);
        await m_unitOfWork.SaveChangesAsync();
        
        // Act
        item.MarkAsWatched();
        item.SetRating(Rating.Create(10).Value!);
        item.SetFavorite(true);
        
        var eventsBeforeSave = item.DomainEvents.ToList();
        eventsBeforeSave.Should().HaveCount(3);
        
        await m_unitOfWork.SaveChangesAsync();
        
        // Assert
        var eventsAfterSave = item.DomainEvents.ToList();
        eventsAfterSave.Should().BeEmpty(); // Events should be cleared after dispatch
        
        // Verify all state changes persisted
        var savedItem = await Context.WatchlistItems
            .FirstOrDefaultAsync(w => w.Id == item.Id);
        
        savedItem.Should().NotBeNull();
        savedItem!.Status.Should().Be(WatchlistStatus.Watched);
        savedItem.UserRating.Should().NotBeNull();
        savedItem.UserRating!.Value.Should().Be(10);
        savedItem.IsFavorite.Should().BeTrue();
        
        await CleanupDatabaseAsync();
    }
    
    [Fact]
    public async Task MultipleItems_EachRaisesOwnEvents()
    {
        // Arrange
        await InitializeDatabaseAsync();
        
        var user = TestDataBuilder.User()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();
        
        var movie1 = TestDataBuilder.Movie().WithTitle("Movie 1").WithTmdbId(1).Build();
        var movie2 = TestDataBuilder.Movie().WithTitle("Movie 2").WithTmdbId(2).Build();
        
        Context.Users.Add(user);
        Context.Movies.AddRange(movie1, movie2);
        await m_unitOfWork.SaveChangesAsync();
        
        var item1 = WatchlistItem.Create(user.Id, movie1.Id, movie1);
        var item2 = WatchlistItem.Create(user.Id, movie2.Id, movie2);
        
        Context.WatchlistItems.AddRange(item1, item2);
        await m_unitOfWork.SaveChangesAsync();
        
        // Act
        item1.MarkAsWatched();
        item2.MarkAsWatched();
        
        await m_unitOfWork.SaveChangesAsync();
        
        // Assert
        item1.DomainEvents.Should().BeEmpty();
        item2.DomainEvents.Should().BeEmpty();
        
        var savedItems = await Context.WatchlistItems
            .Where(w => w.UserId == user.Id)
            .ToListAsync();
        
        savedItems.Should().HaveCount(2);
        savedItems.Should().AllSatisfy(item => item.Status.Should().Be(WatchlistStatus.Watched));
        
        await CleanupDatabaseAsync();
    }
}

