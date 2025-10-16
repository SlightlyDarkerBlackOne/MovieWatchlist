using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Infrastructure.Data;
using MovieWatchlist.Api;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Interfaces;
using FluentAssertions;
using System.Net.Http.Json;
using System.Text.Json;

namespace MovieWatchlist.Tests.Infrastructure;

/// <summary>
/// Enhanced base class for integration tests with comprehensive setup and utilities
/// </summary>
public abstract class EnhancedIntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly MovieWatchlistDbContext Context;
    protected readonly IServiceScope Scope;
    protected readonly JsonSerializerOptions JsonOptions;

    protected EnhancedIntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory
            .ConfigureForIntegrationTesting()
            .WithAuthentication()
            .WithTmdbTestConfig();

        Client = Factory.CreateClient();
        
        // Create a scope to access services
        Scope = Factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<MovieWatchlistDbContext>();
        
        // Configure JSON serialization options
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Initializes the database for testing
    /// </summary>
    protected async Task InitializeDatabaseAsync()
    {
        // For PostgreSQL, delete and recreate the database with migrations
        await Context.Database.EnsureDeletedAsync();
        
        // Use MigrateAsync instead of EnsureCreatedAsync for proper migration handling
        await Context.Database.MigrateAsync();
    }

    /// <summary>
    /// Cleans up the test database
    /// </summary>
    protected async Task CleanupDatabaseAsync()
    {
        await Context.Database.EnsureDeletedAsync();
    }

    #region Authentication Helpers

    /// <summary>
    /// Registers a test user and returns the authentication result
    /// </summary>
    protected async Task<AuthTestResult> RegisterTestUserAsync(
        string username = "testuser", 
        string email = "test@example.com", 
        string password = "TestPassword123!")
    {
        var registerRequest = new
        {
            Username = username,
            Email = email,
            Password = password
        };

        var response = await Client.PostAsJsonAsync("/api/Auth/register", registerRequest);
        response.IsSuccessStatusCode.Should().BeTrue();

        var content = await response.Content.ReadAsStringAsync();
        var authResult = JsonSerializer.Deserialize<AuthTestResult>(content, JsonOptions);
        
        authResult.Should().NotBeNull();
        authResult!.IsSuccess.Should().BeTrue();
        
        return authResult;
    }

    /// <summary>
    /// Logs in a test user and returns the authentication result
    /// </summary>
    protected async Task<AuthTestResult> LoginTestUserAsync(
        string usernameOrEmail = "testuser",
        string password = "TestPassword123!")
    {
        var loginRequest = new
        {
            UsernameOrEmail = usernameOrEmail,
            Password = password
        };

        var response = await Client.PostAsJsonAsync("/api/Auth/login", loginRequest);
        response.IsSuccessStatusCode.Should().BeTrue();

        var content = await response.Content.ReadAsStringAsync();
        var authResult = JsonSerializer.Deserialize<AuthTestResult>(content, JsonOptions);
        
        authResult.Should().NotBeNull();
        authResult!.IsSuccess.Should().BeTrue();
        
        return authResult;
    }

    /// <summary>
    /// Creates an authenticated HTTP client with JWT token
    /// </summary>
    protected async Task<HttpClient> CreateAuthenticatedClientAsync(
        string username = "testuser",
        string password = "TestPassword123!")
    {
        var authResult = await LoginTestUserAsync(username, password);
        
        var authenticatedClient = Factory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResult.Token);
        
        return authenticatedClient;
    }

    #endregion

    #region Database Helpers

    /// <summary>
    /// Seeds the test database with basic test data
    /// </summary>
    protected virtual async Task SeedBasicTestDataAsync()
    {
        // Create test users
        var testUser = TestDataBuilder.User()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .WithPasswordHash("hashed_password")
            .Build();

        var testUser2 = TestDataBuilder.User()
            .WithId(2)
            .WithUsername("testuser2")
            .WithEmail("test2@example.com")
            .WithPasswordHash("hashed_password")
            .Build();

        Context.Users.AddRange(testUser, testUser2);

        // Create test movies
        var movie1 = TestDataBuilder.Movie()
            .WithId(1)
            .WithTmdbId(12345)
            .WithTitle("Test Movie 1")
            .WithGenres("Action", "Drama")
            .Build();

        var movie2 = TestDataBuilder.Movie()
            .WithId(2)
            .WithTmdbId(67890)
            .WithTitle("Test Movie 2")
            .WithGenres("Comedy", "Romance")
            .Build();

        Context.Movies.AddRange(movie1, movie2);

        // Create test watchlist items
        var watchlistItem1 = TestDataBuilder.WatchlistItem()
            .WithUserId(1)
            .WithMovieId(1)
            .WithStatus(WatchlistStatus.Planned)
            .Build();

        var watchlistItem2 = TestDataBuilder.WatchlistItem()
            .WithUserId(1)
            .WithMovieId(2)
            .WithStatus(WatchlistStatus.Watched)
            .WithIsFavorite(true)
            .WithUserRating(8)
            .Build();

        Context.WatchlistItems.AddRange(watchlistItem1, watchlistItem2);

        await Context.SaveChangesAsync();
    }

    /// <summary>
    /// Cleans up the test database
    /// </summary>
    protected virtual async Task CleanupTestDataAsync()
    {
        Context.Database.EnsureDeleted();
        await Task.CompletedTask;
    }

    #endregion

    #region HTTP Helpers

    /// <summary>
    /// Makes a GET request and deserializes the response
    /// </summary>
    protected async Task<T?> GetAsync<T>(string endpoint)
    {
        var response = await Client.GetAsync(endpoint);
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// Makes a POST request and deserializes the response
    /// </summary>
    protected async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        var response = await Client.PostAsJsonAsync(endpoint, data);
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// Makes a PUT request and deserializes the response
    /// </summary>
    protected async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        var response = await Client.PutAsJsonAsync(endpoint, data);
        response.IsSuccessStatusCode.Should().BeTrue();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// Makes a DELETE request
    /// </summary>
    protected async Task<HttpResponseMessage> DeleteAsync(string endpoint)
    {
        var response = await Client.DeleteAsync(endpoint);
        return response;
    }

    #endregion

    public virtual void Dispose()
    {
        Client?.Dispose();
        Context?.Dispose();
        Scope?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Test result for authentication operations
/// </summary>
public class AuthTestResult
{
    public bool IsSuccess { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserTestDto? User { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Test DTO for user data
/// </summary>
public class UserTestDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
