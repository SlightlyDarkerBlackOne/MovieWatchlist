using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Persistence.Data;
using MovieWatchlist.Tests.Shared.TestDataBuilders;
using MovieWatchlist.Tests.Shared.Infrastructure;
using static MovieWatchlist.Tests.Shared.Infrastructure.TestConstants;

namespace MovieWatchlist.Persistence.IntegrationTests;

/// <summary>
/// Integration tests for persistence layer (database operations, EF Core, repositories)
/// </summary>
public class PersistenceIntegrationTests : EnhancedIntegrationTestBase
{
    public PersistenceIntegrationTests(WebApplicationFactory<Program> factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task Database_CanCreateAndDeleteTables_Successfully()
    {
        await InitializeDatabaseAsync();
        
        var canConnect = await Context.Database.CanConnectAsync();
        
        canConnect.Should().BeTrue();
        
        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task TestDataSeeder_CanSeedData_Successfully()
    {
        await InitializeDatabaseAsync();
        
        await SeedBasicTestDataAsync();
        
        var userCount = await Context.Users.CountAsync();
        var movieCount = await Context.Movies.CountAsync();
        var watchlistCount = await Context.WatchlistItems.CountAsync();
        
        userCount.Should().Be(2);
        movieCount.Should().Be(2);
        watchlistCount.Should().Be(2);
        
        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task TestDataBuilder_CreatesValidEntities_Successfully()
    {
        await InitializeDatabaseAsync();
        
        var user = TestDataBuilder.User().Build();
        var movie = TestDataBuilder.Movie().Build();

        Context.Users.Add(user);
        Context.Movies.Add(movie);
        await Context.SaveChangesAsync();

        var savedUser = await Context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        var savedMovie = await Context.Movies.FirstOrDefaultAsync(m => m.Id == movie.Id);
        
        savedUser.Should().NotBeNull();
        savedUser!.Username.Value.Should().Be(user.Username.Value);
        savedUser.Email.Value.Should().Be(user.Email.Value);
        
        savedMovie.Should().NotBeNull();
        savedMovie!.Title.Should().Be(movie.Title);
        savedMovie.TmdbId.Should().Be(movie.TmdbId);
        
        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task TestDatabaseSeeder_CanSeedAllEntitiesWithRelationships_Successfully()
    {
        await InitializeDatabaseAsync();
        
        await TestDatabaseSeeder.SeedTestDataAsync(Context);
        
        var userCount = await Context.Users.CountAsync();
        var movieCount = await Context.Movies.CountAsync();
        var watchlistCount = await Context.WatchlistItems.CountAsync();
        var tokenCount = await Context.RefreshTokens.CountAsync();
        
        userCount.Should().Be(SeederCounts.Users);
        movieCount.Should().Be(SeederCounts.Movies);
        watchlistCount.Should().Be(SeederCounts.WatchlistItems);
        tokenCount.Should().Be(SeederCounts.RefreshTokens);

        var activeUsers = await Context.Users.AsNoTracking().Where(u => u.LastLoginAt != null).ToListAsync();
        var inactiveUser = await Context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == "inactiveuser");
        
        activeUsers.Should().HaveCount(SeederCounts.ActiveUsers);
        inactiveUser.Should().NotBeNull();
        inactiveUser!.LastLoginAt.Should().BeNull();

        var plannedItems = await Context.WatchlistItems.AsNoTracking().Where(w => w.Status == WatchlistStatus.Planned).ToListAsync();
        var watchedItems = await Context.WatchlistItems.AsNoTracking().Where(w => w.Status == WatchlistStatus.Watched).ToListAsync();
        var watchingItems = await Context.WatchlistItems.AsNoTracking().Where(w => w.Status == WatchlistStatus.Watching).ToListAsync();
        
        plannedItems.Should().HaveCount(SeederCounts.PlannedWatchlistItems);
        watchedItems.Should().HaveCount(SeederCounts.WatchedWatchlistItems);
        watchingItems.Should().HaveCount(SeederCounts.WatchingWatchlistItems);

        var favoriteItems = await Context.WatchlistItems.AsNoTracking().Where(w => w.IsFavorite == true).ToListAsync();
        var ratedItems = await Context.WatchlistItems.AsNoTracking().Where(w => w.UserRating != null).ToListAsync();
        var unratedItems = await Context.WatchlistItems.AsNoTracking().Where(w => w.UserRating == null).ToListAsync();
        
        favoriteItems.Should().HaveCount(SeederCounts.FavoriteWatchlistItems);
        ratedItems.Should().HaveCount(SeederCounts.RatedWatchlistItems);
        unratedItems.Should().HaveCount(SeederCounts.UnratedWatchlistItems);

        var firstUser = await Context.Users.AsNoTracking().FirstAsync();
        var user1Watchlist = await Context.WatchlistItems
            .AsNoTracking()
            .Where(w => w.UserId == firstUser.Id)
            .Include(w => w.Movie)
            .ToListAsync();
        
        user1Watchlist.Should().HaveCount(SeederCounts.FirstUserWatchlistItems);
        user1Watchlist.Should().OnlyContain(w => w.Movie != null);
        user1Watchlist.Select(w => w.Movie!.Title).Should().Contain("The Dark Knight", "Inception", "The Shawshank Redemption", "Pulp Fiction");

        var validTokens = await Context.RefreshTokens.AsNoTracking().Where(t => !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow).ToListAsync();
        var expiredTokens = await Context.RefreshTokens.AsNoTracking().Where(t => t.ExpiresAt <= DateTime.UtcNow).ToListAsync();
        var revokedTokens = await Context.RefreshTokens.AsNoTracking().Where(t => t.IsRevoked).ToListAsync();
        
        validTokens.Should().HaveCount(SeederCounts.ValidRefreshTokens);
        expiredTokens.Should().HaveCount(SeederCounts.ExpiredRefreshTokens);
        revokedTokens.Should().HaveCount(SeederCounts.RevokedRefreshTokens);

        var movies = await Context.Movies.AsNoTracking().ToListAsync();
        movies.Should().OnlyContain(m => !string.IsNullOrEmpty(m.Title));
        movies.Should().OnlyContain(m => !string.IsNullOrEmpty(m.Overview));
        movies.Should().OnlyContain(m => m.TmdbId > 0);
        movies.Should().OnlyContain(m => m.VoteAverage >= 0 && m.VoteAverage <= 10);
        movies.Should().OnlyContain(m => m.VoteCount >= 0);

        var moviesWithGenres = await Context.Movies.AsNoTracking().ToListAsync();
        moviesWithGenres.Should().HaveCount(5);
        moviesWithGenres.Should().OnlyContain(m => m.Genres != null && m.Genres.Length > 0 && m.Genres.Any(g => !string.IsNullOrEmpty(g)));
        
        await CleanupDatabaseAsync();
    }

    [Fact]
    public async Task Database_CanHandleTransactions_Successfully()
    {
        await InitializeDatabaseAsync();
        
        using var transaction = await Context.Database.BeginTransactionAsync();
        
        try
        {
            var user = TestDataBuilder.User()
                .Build();
            
            Context.Users.Add(user);
            await Context.SaveChangesAsync();
            
            var userCount = await Context.Users.CountAsync();
            userCount.Should().BeGreaterThan(0);
            
            await transaction.RollbackAsync();
            
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

