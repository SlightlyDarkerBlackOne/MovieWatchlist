using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MovieWatchlist.Persistence.Data;
using MovieWatchlist.Core.Models;

namespace MovieWatchlist.Tests.Infrastructure;

/// <summary>
/// Extension methods for testing utilities
/// </summary>
public static class TestExtensions
{
    /// <summary>
    /// Serializes an object to JSON and creates an HTTP content
    /// </summary>
    public static StringContent ToJsonContent<T>(this T obj)
    {
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Deserializes JSON response content to a specified type
    /// </summary>
    public static async Task<T?> ReadAsJsonAsync<T>(this HttpContent content)
    {
        var json = await content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Adds JWT Bearer token to the request headers
    /// </summary>
    public static void AddBearerToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Removes JWT Bearer token from the request headers
    /// </summary>
    public static void RemoveBearerToken(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Creates a test web application factory with in-memory database
    /// </summary>
    public static WebApplicationFactory<T> WithInMemoryDatabase<T>(
        this WebApplicationFactory<T> factory, 
        string? databaseName = null) where T : class
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real database
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<MovieWatchlistDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database
                services.AddDbContext<MovieWatchlistDbContext>(options =>
                {
                    options.UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString());
                });
            });

            builder.UseEnvironment("Testing");
        });
    }

    /// <summary>
    /// Ensures the database is created and seeded with test data
    /// </summary>
    public static async Task EnsureDatabaseSeededAsync(this MovieWatchlistDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
        await TestDatabaseSeeder.SeedTestDataAsync(context);
    }

    /// <summary>
    /// Clears all data from the database
    /// </summary>
    public static async Task ClearDatabaseAsync(this MovieWatchlistDbContext context)
    {
        await TestDatabaseSeeder.ClearTestDataAsync(context);
    }

    /// <summary>
    /// Gets a user by ID from the context
    /// </summary>
    public static async Task<User?> GetUserAsync(this MovieWatchlistDbContext context, int userId)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    /// <summary>
    /// Gets a movie by ID from the context
    /// </summary>
    public static async Task<Movie?> GetMovieAsync(this MovieWatchlistDbContext context, int movieId)
    {
        return await context.Movies.FirstOrDefaultAsync(m => m.Id == movieId);
    }

    /// <summary>
    /// Gets watchlist items for a user from the context
    /// </summary>
    public static async Task<List<WatchlistItem>> GetUserWatchlistAsync(this MovieWatchlistDbContext context, int userId)
    {
        return await context.WatchlistItems
            .Where(w => w.UserId == userId)
            .Include(w => w.Movie)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a watchlist item by ID from the context
    /// </summary>
    public static async Task<WatchlistItem?> GetWatchlistItemAsync(this MovieWatchlistDbContext context, int itemId)
    {
        return await context.WatchlistItems
            .Include(w => w.Movie)
            .FirstOrDefaultAsync(w => w.Id == itemId);
    }

    /// <summary>
    /// Gets refresh tokens for a user from the context
    /// </summary>
    public static async Task<List<RefreshToken>> GetUserRefreshTokensAsync(this MovieWatchlistDbContext context, int userId)
    {
        return await context.RefreshTokens
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    /// <summary>
    /// Verifies that an HTTP response has the expected status code
    /// </summary>
    public static void ShouldHaveStatusCode(this HttpResponseMessage response, int expectedStatusCode)
    {
        if (response.StatusCode != (System.Net.HttpStatusCode)expectedStatusCode)
        {
            throw new Exception($"Expected status code {expectedStatusCode}, but got {(int)response.StatusCode}. " +
                              $"Response: {response.Content.ReadAsStringAsync().Result}");
        }
    }

    /// <summary>
    /// Verifies that an HTTP response has a successful status code (200-299)
    /// </summary>
    public static void ShouldBeSuccessful(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Expected successful status code, but got {(int)response.StatusCode}. " +
                              $"Response: {response.Content.ReadAsStringAsync().Result}");
        }
    }

    /// <summary>
    /// Verifies that an HTTP response has an error status code (400+)
    /// </summary>
    public static void ShouldBeError(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            throw new Exception($"Expected error status code, but got successful {(int)response.StatusCode}. " +
                              $"Response: {response.Content.ReadAsStringAsync().Result}");
        }
    }

    /// <summary>
    /// Gets the count of entities in the database
    /// </summary>
    public static async Task<int> GetEntityCountAsync<T>(this MovieWatchlistDbContext context) where T : class
    {
        return await context.Set<T>().CountAsync();
    }

    /// <summary>
    /// Checks if an entity exists in the database
    /// </summary>
    public static async Task<bool> EntityExistsAsync<T>(this MovieWatchlistDbContext context, System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
    {
        return await context.Set<T>().AnyAsync(predicate);
    }
}
