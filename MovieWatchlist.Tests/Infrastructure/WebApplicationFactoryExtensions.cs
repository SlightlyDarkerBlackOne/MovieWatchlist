using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MovieWatchlist.Infrastructure.Data;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Core.Configuration;
using MovieWatchlist.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace MovieWatchlist.Tests.Infrastructure;

/// <summary>
/// Extension methods for configuring WebApplicationFactory for integration tests
/// </summary>
public static class WebApplicationFactoryExtensions
{
    /// <summary>
    /// Configures the WebApplicationFactory for integration testing with PostgreSQL database
    /// </summary>
    public static WebApplicationFactory<T> ConfigureForIntegrationTesting<T>(
        this WebApplicationFactory<T> factory,
        string? databaseName = null) where T : class
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing database configuration
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<MovieWatchlistDbContext>));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                // Add PostgreSQL database for testing
                services.AddDbContext<MovieWatchlistDbContext>(options =>
                {
                    var dbName = databaseName ?? $"test_db_{Guid.NewGuid():N}";
                    var connectionString = $"Host=localhost;Database={dbName};Username=postgres;Password=password;Port=5432;";
                    
                    options.UseNpgsql(connectionString);
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });

                // Configure test-friendly logging
                services.AddLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Warning);
                });

                // Override external services with test-friendly versions
                ConfigureTestServices(services);
            });

            builder.UseEnvironment("Testing");
        });
    }

    /// <summary>
    /// Configures services for testing by replacing external dependencies with test-friendly versions
    /// </summary>
    private static void ConfigureTestServices(IServiceCollection services)
    {
        // Mock external services that shouldn't make real API calls during tests
        // Note: We'll keep the real services for now, but this is where we could add mocks
        // if needed for services like TmdbService, etc.
    }

    /// <summary>
    /// Adds authentication test configuration
    /// </summary>
    public static WebApplicationFactory<T> WithAuthentication<T>(
        this WebApplicationFactory<T> factory,
        string? testSecretKey = null) where T : class
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Override JWT settings for testing
                var jwtSettings = new JwtSettings
                {
                    SecretKey = testSecretKey ?? "test_secret_key_that_is_at_least_32_characters_long_for_testing",
                    ExpirationMinutes = 60,
                    RefreshTokenExpirationDays = 7,
                    Issuer = "MovieWatchlistAPI-Test",
                    Audience = "MovieWatchlistUsers-Test"
                };

                // Remove existing JWT settings
                var jwtDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IOptions<JwtSettings>));
                if (jwtDescriptor != null)
                {
                    services.Remove(jwtDescriptor);
                }

                // Add test JWT settings
                services.AddSingleton(Options.Create(jwtSettings));
            });
        });
    }

    /// <summary>
    /// Adds TMDB test configuration
    /// </summary>
    public static WebApplicationFactory<T> WithTmdbTestConfig<T>(
        this WebApplicationFactory<T> factory,
        string? testApiKey = null) where T : class
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Override TMDB settings for testing
                var tmdbSettings = new TmdbSettings
                {
                    ApiKey = testApiKey ?? "test_api_key",
                    ImageBaseUrl = "https://image.tmdb.org/t/p"
                };

                // Remove existing TMDB settings
                var tmdbDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IOptions<TmdbSettings>));
                if (tmdbDescriptor != null)
                {
                    services.Remove(tmdbDescriptor);
                }

                // Add test TMDB settings
                services.AddSingleton(Options.Create(tmdbSettings));
            });
        });
    }
}
