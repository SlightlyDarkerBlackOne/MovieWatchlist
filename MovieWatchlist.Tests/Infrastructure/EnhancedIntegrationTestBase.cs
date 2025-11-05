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
using System.Net;
using System.Linq;

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

        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            HandleCookies = true
        });
        
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
    /// Creates an authenticated HTTP client by extracting JWT token from Set-Cookie header
    /// and using it as Authorization header (JWT Bearer supports both cookies and headers)
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

        var loginClient = Factory.CreateClient();
        loginClient.BaseAddress = Factory.Server.BaseAddress;
        
        var loginResponse = await loginClient.PostAsJsonAsync(TestConstants.ApiEndpoints.AuthLogin, loginRequest);
        loginResponse.IsSuccessStatusCode.Should().BeTrue();

        string? accessToken = null;
        if (loginResponse.Headers.Contains(TestConstants.HttpHeaders.SetCookie))
        {
            var cookieHeaders = loginResponse.Headers.GetValues(TestConstants.HttpHeaders.SetCookie);
            foreach (var cookieHeader in cookieHeaders)
            {
                if (cookieHeader.Contains("accessToken="))
                {
                    var cookieParts = cookieHeader.Split(';');
                    if (cookieParts.Length > 0)
                    {
                        var nameValue = cookieParts[0].Trim();
                        if (nameValue.StartsWith("accessToken="))
                        {
                            accessToken = System.Net.WebUtility.UrlDecode(nameValue.Substring("accessToken=".Length));
                            break;
                        }
                    }
                }
            }
        }

        var authenticatedClient = Factory.CreateClient();
        authenticatedClient.BaseAddress = Factory.Server.BaseAddress;
        
        if (!string.IsNullOrEmpty(accessToken))
        {
            authenticatedClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }

        return authenticatedClient;
    }

    #endregion

    #region Database Helpers

    /// <summary>
    /// Seeds the test database with basic test data
    /// </summary>
    protected virtual async Task SeedBasicTestDataAsync(int? userId = null)
    {
        int actualUserId = userId ?? 1;
        
        var existingUser = await Context.Users.FirstOrDefaultAsync(u => u.Id == actualUserId);
        if (existingUser == null)
        {
            var testUser = User.Create(
                Username.Create("testuser").Value!,
                Email.Create("test@example.com").Value!,
                "hashed_password"
            );
            typeof(User).GetProperty("Id")!.SetValue(testUser, actualUserId);
            Context.Users.Add(testUser);
        }
        else
        {
            actualUserId = existingUser.Id;
        }

        var testUser2 = User.Create(
            Username.Create("testuser2").Value!,
            Email.Create("test2@example.com").Value!,
            "hashed_password"
        );
        typeof(User).GetProperty("Id")!.SetValue(testUser2, actualUserId + 1);

        if (!Context.Users.Any(u => u.Id == testUser2.Id))
        {
            Context.Users.Add(testUser2);
        }

        await Context.SaveChangesAsync();

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

        if (!Context.Movies.Any(m => m.TmdbId == movie1.TmdbId))
        {
            Context.Movies.Add(movie1);
        }
        else
        {
            movie1 = await Context.Movies.FirstAsync(m => m.TmdbId == movie1.TmdbId);
        }

        if (!Context.Movies.Any(m => m.TmdbId == movie2.TmdbId))
        {
            Context.Movies.Add(movie2);
        }
        else
        {
            movie2 = await Context.Movies.FirstAsync(m => m.TmdbId == movie2.TmdbId);
        }

        await Context.SaveChangesAsync();

        if (!Context.WatchlistItems.Any(w => w.UserId == actualUserId && w.MovieId == movie1.Id))
        {
            var watchlistItem1 = WatchlistItem.Create(actualUserId, movie1);
            watchlistItem1.UpdateStatus(WatchlistStatus.Planned);
            Context.WatchlistItems.Add(watchlistItem1);
        }

        if (!Context.WatchlistItems.Any(w => w.UserId == actualUserId && w.MovieId == movie2.Id))
        {
            var watchlistItem2 = WatchlistItem.Create(actualUserId, movie2);
            watchlistItem2.UpdateStatus(WatchlistStatus.Watched);
            watchlistItem2.ToggleFavorite();
            watchlistItem2.SetRating(Rating.Create(8).Value!);
            Context.WatchlistItems.Add(watchlistItem2);
        }

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


