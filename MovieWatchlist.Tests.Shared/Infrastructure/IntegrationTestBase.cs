using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Persistence.Data;
using MovieWatchlist.Api;
using Xunit;

namespace MovieWatchlist.Tests.Shared.Infrastructure;

/// <summary>
/// Base class for integration tests that provides a test web application factory
/// with in-memory database and proper service configuration.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly MovieWatchlistDbContext Context;
    protected readonly IServiceScope Scope;

    protected IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        Factory = factory.WithWebHostBuilder(builder =>
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
                    options.UseInMemoryDatabase(Guid.NewGuid().ToString());
                });

                // Configure logging for tests
                services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));
            });

            builder.UseEnvironment("Testing");
        });

        Client = Factory.CreateClient();
        
        // Create a scope to access services
        Scope = Factory.Services.CreateScope();
        Context = Scope.ServiceProvider.GetRequiredService<MovieWatchlistDbContext>();
        
        // Ensure database is created
        Context.Database.EnsureCreated();
    }

    /// <summary>
    /// Seeds the test database with initial data
    /// </summary>
    protected virtual async Task SeedTestDataAsync()
    {
        // Override in derived classes to add specific test data
        await Context.SaveChangesAsync();
    }

    /// <summary>
    /// Cleans up the test database
    /// </summary>
    protected virtual Task CleanupTestDataAsync()
    {
        // Override in derived classes for custom cleanup
        Context.Database.EnsureDeleted();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Client?.Dispose();
        Context?.Dispose();
        Scope?.Dispose();
        GC.SuppressFinalize(this);
    }
}
