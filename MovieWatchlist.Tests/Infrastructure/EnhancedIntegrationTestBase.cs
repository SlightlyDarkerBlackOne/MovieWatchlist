using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Infrastructure.Data;
using MovieWatchlist.Api;
using MovieWatchlist.Api.DTOs;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Core.ValueObjects;
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
        var databaseName = $"test_db_{Guid.NewGuid():N}";
        
        Factory = factory
            .ConfigureForIntegrationTesting(databaseName)
            .WithAuthentication()
            .WithTmdbTestConfig();

        Client = Factory.CreateClient();
        
        Scope = Factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<MovieWatchlistDbContext>();
        
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
        try
        {
            await Context.Database.EnsureDeletedAsync();
            await Context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to initialize test database", ex);
        }
    }

    /// <summary>
    /// Cleans up the test database
    /// </summary>
    protected async Task CleanupDatabaseAsync()
    {
        try
        {
            await Context.Database.EnsureDeletedAsync();
        }
        catch (Exception)
        {
        }
    }

    #region Authentication Helpers

    /// <summary>
    /// Registers a test user and returns the authentication result
    /// </summary>
    protected async Task<AuthTestResult> RegisterTestUserAsync(
        string username = TestConstants.Users.DefaultUsername, 
        string email = TestConstants.Users.DefaultEmail, 
        string password = TestConstants.Users.DefaultPassword)
    {
        var registerRequest = new
        {
            Username = username,
            Email = email,
            Password = password
        };

        var response = await Client.PostAsJsonAsync(TestConstants.ApiEndpoints.AuthRegister, registerRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Registration failed with status {response.StatusCode}: {errorContent}");
        }
        
        response.IsSuccessStatusCode.Should().BeTrue();

        var cookies = response.Headers.GetValues(TestConstants.HttpHeaders.SetCookie).ToList();
        cookies.Should().Contain(c => c.Contains(TestConstants.CookieNames.AccessToken));
        cookies.Should().Contain(c => c.Contains(TestConstants.CookieNames.RefreshToken));

        var content = await response.Content.ReadAsStringAsync();
        var responseDto = JsonSerializer.Deserialize<RegisterResponse>(content, JsonOptions);
        
        responseDto.Should().NotBeNull();
        responseDto!.User.Should().NotBeNull();
        
        return new AuthTestResult
        {
            IsSuccess = true,
            User = new UserTestDto
            {
                Id = responseDto.User.Id,
                Username = responseDto.User.Username,
                Email = responseDto.User.Email,
                CreatedAt = responseDto.User.CreatedAt
            },
            ExpiresAt = responseDto.ExpiresAt
        };
    }

    /// <summary>
    /// Logs in a test user and returns the authentication result
    /// </summary>
    protected async Task<AuthTestResult> LoginTestUserAsync(
        string usernameOrEmail = TestConstants.Users.DefaultUsername,
        string password = TestConstants.Users.DefaultPassword)
    {
        var loginRequest = new
        {
            UsernameOrEmail = usernameOrEmail,
            Password = password
        };

        var response = await Client.PostAsJsonAsync(TestConstants.ApiEndpoints.AuthLogin, loginRequest);
        response.IsSuccessStatusCode.Should().BeTrue();

        var cookies = response.Headers.GetValues(TestConstants.HttpHeaders.SetCookie).ToList();
        cookies.Should().Contain(c => c.Contains(TestConstants.CookieNames.AccessToken));
        cookies.Should().Contain(c => c.Contains(TestConstants.CookieNames.RefreshToken));

        var content = await response.Content.ReadAsStringAsync();
        var responseDto = JsonSerializer.Deserialize<LoginResponse>(content, JsonOptions);
        
        responseDto.Should().NotBeNull();
        responseDto!.User.Should().NotBeNull();
        
        return new AuthTestResult
        {
            IsSuccess = true,
            User = new UserTestDto
            {
                Id = responseDto.User.Id,
                Username = responseDto.User.Username,
                Email = responseDto.User.Email,
                CreatedAt = responseDto.User.CreatedAt
            },
            ExpiresAt = responseDto.ExpiresAt
        };
    }

    /// <summary>
    /// Creates an authenticated HTTP client with cookies
    /// </summary>
    protected async Task<HttpClient> CreateAuthenticatedClientAsync(
        string username = TestConstants.Users.DefaultUsername,
        string password = TestConstants.Users.DefaultPassword)
    {
        var loginRequest = new
        {
            UsernameOrEmail = username,
            Password = password
        };

        var loginResponse = await Client.PostAsJsonAsync(TestConstants.ApiEndpoints.AuthLogin, loginRequest);
        loginResponse.IsSuccessStatusCode.Should().BeTrue();

        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new System.Net.CookieContainer()
        };

        var baseUri = Client.BaseAddress ?? new Uri("http://localhost");
        var authenticatedClient = new HttpClient(handler)
        {
            BaseAddress = baseUri
        };

        var cookies = loginResponse.Headers.GetValues(TestConstants.HttpHeaders.SetCookie);
        foreach (var cookieHeader in cookies)
        {
            var cookieParts = cookieHeader.Split(';');
            if (cookieParts.Length > 0)
            {
                var nameValue = cookieParts[0].Trim();
                var nameValueParts = nameValue.Split('=', 2);
                if (nameValueParts.Length == 2)
                {
                    var cookieName = nameValueParts[0].Trim();
                    var cookieValue = nameValueParts[1].Trim();
                    var cookie = new System.Net.Cookie(cookieName, cookieValue)
                    {
                        Domain = baseUri.Host,
                        Path = "/",
                        HttpOnly = cookieHeader.Contains("HttpOnly", StringComparison.OrdinalIgnoreCase),
                        Secure = cookieHeader.Contains("Secure", StringComparison.OrdinalIgnoreCase)
                    };
                    handler.CookieContainer.Add(baseUri, cookie);
                }
            }
        }
        
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
        var testUser = User.Create(
            Username.Create("testuser").Value!,
            Email.Create("test@example.com").Value!,
            "hashed_password"
        );
        typeof(User).GetProperty("Id")!.SetValue(testUser, 1);

        var testUser2 = User.Create(
            Username.Create("testuser2").Value!,
            Email.Create("test2@example.com").Value!,
            "hashed_password"
        );
        typeof(User).GetProperty("Id")!.SetValue(testUser2, 2);

        Context.Users.AddRange(testUser, testUser2);
        await Context.SaveChangesAsync(); // Save users first to get their IDs

        // Create test movies
        var movie1 = TestDataBuilder.Movie()
            .WithTmdbId(12345)
            .WithTitle("Test Movie 1")
            .WithGenres("Action", "Drama")
            .Build();

        var movie2 = TestDataBuilder.Movie()
            .WithTmdbId(67890)
            .WithTitle("Test Movie 2")
            .WithGenres("Comedy", "Romance")
            .Build();

        Context.Movies.AddRange(movie1, movie2);
        await Context.SaveChangesAsync(); // Save movies to get their IDs

        var watchlistItem1 = WatchlistItem.Create(testUser.Id, movie1);
        watchlistItem1.UpdateStatus(WatchlistStatus.Planned);

        var watchlistItem2 = WatchlistItem.Create(testUser.Id, movie2);
        watchlistItem2.UpdateStatus(WatchlistStatus.Watched);
        watchlistItem2.ToggleFavorite();
        watchlistItem2.SetRating(Rating.Create(8).Value!);
        
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
        try
        {
            Context?.Database.EnsureDeleted();
        }
        catch (Exception)
        {
        }
        
        Client?.Dispose();
        Context?.Dispose();
        Scope?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Test result for authentication operations
/// Note: Tokens are now in httpOnly cookies, not in response body
/// </summary>
public class AuthTestResult
{
    public bool IsSuccess { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserTestDto? User { get; set; }
    public string? ErrorMessage { get; set; }
    
    [Obsolete("Tokens are now in httpOnly cookies. Use cookies from Set-Cookie header instead.")]
    public string? Token { get; set; }
    
    [Obsolete("Tokens are now in httpOnly cookies. Use cookies from Set-Cookie header instead.")]
    public string? RefreshToken { get; set; }
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
