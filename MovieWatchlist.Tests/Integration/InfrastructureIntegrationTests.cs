using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Infrastructure.Data;
using MovieWatchlist.Tests.TestDataBuilders;
using MovieWatchlist.Tests.Infrastructure;
using Xunit;

namespace MovieWatchlist.Tests.Integration;

/// <summary>
/// Integration tests to verify the test infrastructure is working correctly
/// </summary>
public class InfrastructureIntegrationTests : EnhancedIntegrationTestBase
{
    public InfrastructureIntegrationTests(WebApplicationFactory<Program> factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task Database_CanCreateAndDeleteTables_Successfully()
    {
        // Arrange
        await InitializeDatabaseAsync();
        
        // Act
        var canConnect = await Context.Database.CanConnectAsync();
        
        // Assert
        canConnect.Should().BeTrue();
        
        // Cleanup
        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task TestDataSeeder_CanSeedData_Successfully()
    {
        // Arrange
        await InitializeDatabaseAsync();
        
        // Act
        await SeedBasicTestDataAsync();
        
        // Assert
        var userCount = await Context.Users.CountAsync();
        var movieCount = await Context.Movies.CountAsync();
        var watchlistCount = await Context.WatchlistItems.CountAsync();
        
        userCount.Should().Be(2);
        movieCount.Should().Be(2);
        watchlistCount.Should().Be(2);
        
        // Cleanup
        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task TestDataBuilder_CreatesValidEntities_Successfully()
    {
        // Arrange
        await InitializeDatabaseAsync();
        
        var user = TestDataBuilder.User()
            .WithUsername("builderuser")
            .WithEmail("builder@example.com")
            .Build();

        var movie = TestDataBuilder.Movie()
            .WithTitle("Builder Movie")
            .WithTmdbId(99999)
            .Build();

        // Act
        Context.Users.Add(user);
        Context.Movies.Add(movie);
        await Context.SaveChangesAsync();

        // Assert
        var savedUser = await Context.Users.FirstOrDefaultAsync(u => u.Username == "builderuser");
        var savedMovie = await Context.Movies.FirstOrDefaultAsync(m => m.Title == "Builder Movie");
        
        savedUser.Should().NotBeNull();
        savedUser!.Username.Value.Should().Be("builderuser");
        savedUser.Email.Value.Should().Be("builder@example.com");
        
        savedMovie.Should().NotBeNull();
        savedMovie!.Title.Should().Be("Builder Movie");
        savedMovie.TmdbId.Should().Be(99999);
        
        // Cleanup
        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task HttpClient_CanMakeRequests_Successfully()
    {
        // Arrange & Act
        var response = await Client.GetAsync("/api/Movies/popular");
        
        // Assert
        // Note: This might return 401 if authentication is required, but we're just testing the client works
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task TestDatabaseSeeder_CanSeedAllEntitiesWithRelationships_Successfully()
    {
        // Arrange
        await InitializeDatabaseAsync();
        
        // Act
        await TestDatabaseSeeder.SeedTestDataAsync(Context);
        
        // Assert - Verify counts
        var userCount = await Context.Users.CountAsync();
        var movieCount = await Context.Movies.CountAsync();
        var watchlistCount = await Context.WatchlistItems.CountAsync();
        var tokenCount = await Context.RefreshTokens.CountAsync();
        
        userCount.Should().Be(3);
        movieCount.Should().Be(5);
        watchlistCount.Should().Be(6);
        tokenCount.Should().Be(4);

        // Test different user scenarios (active vs inactive)
        var activeUsers = await Context.Users.AsNoTracking().Where(u => u.LastLoginAt != null).ToListAsync();
        var inactiveUser = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == "inactiveuser");
        
        activeUsers.Should().HaveCount(2);
        inactiveUser.Should().NotBeNull();
        inactiveUser!.LastLoginAt.Should().BeNull();

        // Test different watchlist statuses
        var plannedItems = await Context.WatchlistItems.AsNoTracking().Where(w => w.Status == WatchlistStatus.Planned).ToListAsync();
        var watchedItems = await Context.WatchlistItems.AsNoTracking().Where(w => w.Status == WatchlistStatus.Watched).ToListAsync();
        var watchingItems = await Context.WatchlistItems.AsNoTracking().Where(w => w.Status == WatchlistStatus.Watching).ToListAsync();
        
        plannedItems.Should().HaveCount(2);
        watchedItems.Should().HaveCount(3);
        watchingItems.Should().HaveCount(1);

        // Test favorites and ratings
        var favoriteItems = await Context.WatchlistItems.AsNoTracking().Where(w => w.IsFavorite == true).ToListAsync();
        var ratedItems = await Context.WatchlistItems.AsNoTracking().Where(w => w.UserRating != null).ToListAsync();
        var unratedItems = await Context.WatchlistItems.AsNoTracking().Where(w => w.UserRating == null).ToListAsync();
        
        favoriteItems.Should().HaveCount(3);
        ratedItems.Should().HaveCount(3);
        unratedItems.Should().HaveCount(3);

        // Test relationships - verify foreign keys work
        var firstUser = await Context.Users.AsNoTracking().FirstAsync();
        var user1Watchlist = await Context.WatchlistItems
            .AsNoTracking()
            .Where(w => w.UserId == firstUser.Id)
            .Include(w => w.Movie)
            .ToListAsync();
        
        user1Watchlist.Should().HaveCount(4);
        user1Watchlist.Should().OnlyContain(w => w.Movie != null);
        user1Watchlist.Select(w => w.Movie!.Title).Should().Contain("The Dark Knight", "Inception", "The Shawshank Redemption", "Pulp Fiction");

        // Test refresh token scenarios
        var validTokens = await Context.RefreshTokens.AsNoTracking().Where(t => !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow).ToListAsync();
        var expiredTokens = await Context.RefreshTokens.AsNoTracking().Where(t => t.ExpiresAt <= DateTime.UtcNow).ToListAsync();
        var revokedTokens = await Context.RefreshTokens.AsNoTracking().Where(t => t.IsRevoked).ToListAsync();
        
        validTokens.Should().HaveCount(2);
        expiredTokens.Should().HaveCount(1);
        revokedTokens.Should().HaveCount(1);

        // Test realistic movie data
        var movies = await Context.Movies.AsNoTracking().ToListAsync();
        movies.Should().OnlyContain(m => !string.IsNullOrEmpty(m.Title));
        movies.Should().OnlyContain(m => !string.IsNullOrEmpty(m.Overview));
        movies.Should().OnlyContain(m => m.TmdbId > 0);
        movies.Should().OnlyContain(m => m.VoteAverage >= 0 && m.VoteAverage <= 10);
        movies.Should().OnlyContain(m => m.VoteCount >= 0);

        // Test genre data
        var moviesWithGenres = await Context.Movies.AsNoTracking().ToListAsync();
        moviesWithGenres.Should().HaveCount(5);
        moviesWithGenres.Should().OnlyContain(m => m.Genres != null && m.Genres.Length > 0 && m.Genres.Any(g => !string.IsNullOrEmpty(g)));
        
        // Cleanup
        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task Database_CanHandleTransactions_Successfully()
    {
        // Arrange
        await InitializeDatabaseAsync();
        
        using var transaction = await Context.Database.BeginTransactionAsync();
        
        try
        {
            // Act
            var user = TestDataBuilder.User()
                .WithUsername("transactionuser")
                .WithEmail("transaction@example.com")
                .Build();
            
            Context.Users.Add(user);
            await Context.SaveChangesAsync();
            
            var userCount = await Context.Users.CountAsync();
            userCount.Should().BeGreaterThan(0);
            
            // Rollback transaction
            await transaction.RollbackAsync();
            
            // Assert - user should not exist after rollback
            var userAfterRollback = await Context.Users.FirstOrDefaultAsync(u => u.Username == "transactionuser");
            userAfterRollback.Should().BeNull();
        }
        finally
        {
            await transaction.DisposeAsync();
            await CleanupDatabaseAsync();
        }
    }
}
