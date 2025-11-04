using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Api;
using MovieWatchlist.Api.DTOs;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Queries;
using MovieWatchlist.Tests.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using static MovieWatchlist.Tests.Infrastructure.TestConstants;

namespace MovieWatchlist.Tests.Controllers;

public class WatchlistControllerTests : EnhancedIntegrationTestBase
{
    public WatchlistControllerTests(WebApplicationFactory<Program> factory) 
        : base(factory)
    {
    }

    [Fact]
    public async Task GetMyWatchlist_WithValidAuth_ReturnsWatchlist()
    {
        await InitializeDatabaseAsync();
        try
        {
            var authResult = await RegisterTestUserAsync();
            var authenticatedClient = await CreateAuthenticatedClientAsync();
            await SeedBasicTestDataAsync(authResult.User!.Id);

            var response = await authenticatedClient.GetAsync(ApiEndpoints.WatchlistMe);
            
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var watchlist = await response.Content.ReadFromJsonAsync<List<WatchlistItem>>();
            watchlist.Should().NotBeNull();
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }

    [Fact]
    public async Task GetMyWatchlist_WithoutAuth_ReturnsUnauthorized()
    {
        await InitializeDatabaseAsync();
        try
        {
            var response = await Client.GetAsync(ApiEndpoints.WatchlistMe);
            
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }

    [Fact]
    public async Task GetMyStatistics_WithValidAuth_ReturnsStatistics()
    {
        await InitializeDatabaseAsync();
        try
        {
            var authResult = await RegisterTestUserAsync();
            var authenticatedClient = await CreateAuthenticatedClientAsync();
            await SeedBasicTestDataAsync(authResult.User!.Id);

            var response = await authenticatedClient.GetAsync(ApiEndpoints.WatchlistMeStatistics);
            
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var statistics = await response.Content.ReadFromJsonAsync<WatchlistStatistics>();
            statistics.Should().NotBeNull();
            statistics!.TotalMovies.Should().BeGreaterThan(0);
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }

    [Fact]
    public async Task GetMyStatistics_WithoutAuth_ReturnsUnauthorized()
    {
        await InitializeDatabaseAsync();
        try
        {
            var response = await Client.GetAsync(ApiEndpoints.WatchlistMeStatistics);
            
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }

    [Fact]
    public async Task AddToMyWatchlist_WithValidAuth_CreatesWatchlistItem()
    {
        await InitializeDatabaseAsync();
        try
        {
            var authResult = await RegisterTestUserAsync();
            var authenticatedClient = await CreateAuthenticatedClientAsync();
            await SeedBasicTestDataAsync(authResult.User!.Id);

            var addRequest = new AddToWatchlistDto(
                MovieId: Movies.DefaultTmdbId,
                Status: WatchlistStatus.Planned,
                Notes: "Test note"
            );

            var response = await authenticatedClient.PostAsJsonAsync(ApiEndpoints.WatchlistMeAdd, addRequest);
            
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var watchlistItem = await response.Content.ReadFromJsonAsync<WatchlistItem>();
            watchlistItem.Should().NotBeNull();
            watchlistItem!.Movie.TmdbId.Should().Be(Movies.DefaultTmdbId);
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }

    [Fact]
    public async Task AddToMyWatchlist_WithoutAuth_ReturnsUnauthorized()
    {
        await InitializeDatabaseAsync();
        try
        {
            var addRequest = new AddToWatchlistDto(
                MovieId: Movies.DefaultTmdbId,
                Status: WatchlistStatus.Planned
            );

            var response = await Client.PostAsJsonAsync(ApiEndpoints.WatchlistMeAdd, addRequest);
            
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }

    [Fact]
    public async Task UpdateMyWatchlistItem_WithValidAuth_UpdatesItem()
    {
        await InitializeDatabaseAsync();
        try
        {
            var authResult = await RegisterTestUserAsync();
            var authenticatedClient = await CreateAuthenticatedClientAsync();
            await SeedBasicTestDataAsync(authResult.User!.Id);

            var watchlistItems = await Context.WatchlistItems
                .Where(w => w.UserId == authResult.User!.Id)
                .ToListAsync();
            var firstItemId = watchlistItems.FirstOrDefault()?.Id ?? 1;

            var updateRequest = new UpdateWatchlistItemDto
            {
                Status = WatchlistStatus.Watched,
                IsFavorite = true,
                UserRating = 8
            };

            var response = await authenticatedClient.PutAsJsonAsync(
                ApiEndpoints.WatchlistMeItem.Replace("{watchlistItemId}", firstItemId.ToString()), 
                updateRequest);
            
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var watchlistItem = await response.Content.ReadFromJsonAsync<WatchlistItem>();
            watchlistItem.Should().NotBeNull();
            watchlistItem!.Status.Should().Be(WatchlistStatus.Watched);
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }

    [Fact]
    public async Task UpdateMyWatchlistItem_WithoutAuth_ReturnsUnauthorized()
    {
        await InitializeDatabaseAsync();
        try
        {
            var updateRequest = new UpdateWatchlistItemDto
            {
                Status = WatchlistStatus.Watched
            };

            var response = await Client.PutAsJsonAsync(
                ApiEndpoints.WatchlistMeItem.Replace("{watchlistItemId}", WatchlistItems.FirstItemId.ToString()), 
                updateRequest);
            
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }

    [Fact]
    public async Task RemoveFromMyWatchlist_WithValidAuth_DeletesItem()
    {
        await InitializeDatabaseAsync();
        try
        {
            var authResult = await RegisterTestUserAsync();
            var authenticatedClient = await CreateAuthenticatedClientAsync();
            await SeedBasicTestDataAsync(authResult.User!.Id);

            var watchlistItems = await Context.WatchlistItems
                .Where(w => w.UserId == authResult.User!.Id)
                .ToListAsync();
            var firstItemId = watchlistItems.FirstOrDefault()?.Id ?? 1;

            var response = await authenticatedClient.DeleteAsync(
                ApiEndpoints.WatchlistMeItem.Replace("{watchlistItemId}", firstItemId.ToString()));
            
            response.IsSuccessStatusCode.Should().BeTrue();
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }

    [Fact]
    public async Task RemoveFromMyWatchlist_WithoutAuth_ReturnsUnauthorized()
    {
        await InitializeDatabaseAsync();
        try
        {
            var response = await Client.DeleteAsync(
                ApiEndpoints.WatchlistMeItem.Replace("{watchlistItemId}", WatchlistItems.FirstItemId.ToString()));
            
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }
}

