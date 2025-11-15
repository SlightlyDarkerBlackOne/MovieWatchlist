using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using MovieWatchlist.Application.Features.Watchlist.Commands.AddToWatchlist;
using MovieWatchlist.Application.Features.Watchlist.Commands.UpdateWatchlistItem;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Tests.Shared.Infrastructure;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using static MovieWatchlist.Tests.Shared.Infrastructure.TestConstants;

namespace MovieWatchlist.Api.IntegrationTests.Controllers;

public class WatchlistControllerTests : EnhancedIntegrationTestBase
{
    public WatchlistControllerTests(WebApplicationFactory<Program> factory) 
        : base(factory)
    {
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
    public async Task AddToMyWatchlist_WithoutAuth_ReturnsUnauthorized()
    {
        await InitializeDatabaseAsync();
        try
        {
            var addRequest = new AddToWatchlistCommand(
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
    public async Task UpdateMyWatchlistItem_WithoutAuth_ReturnsUnauthorized()
    {
        await InitializeDatabaseAsync();
        try
        {
            var updateRequest = new UpdateWatchlistItemCommand(
                WatchlistItemId: WatchlistItems.FirstItemId,
                Status: WatchlistStatus.Watched
            );

            var response = await Client.PutAsJsonAsync(
                ApiEndpoints.WatchlistMeItem, 
                updateRequest);
            
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
                $"{ApiEndpoints.WatchlistMeItem}/{WatchlistItems.FirstItemId}");
            
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        finally
        {
            await CleanupDatabaseAsync();
        }
    }
}

