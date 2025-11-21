using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieWatchlist.Core.Interfaces;
using MovieWatchlist.Persistence.Data;
using MovieWatchlist.Persistence.Repositories;

namespace MovieWatchlist.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    private const string DATABASE_CONNECTION_STRING_ENV = "DATABASE_CONNECTION_STRING";
    private const string DEFAULT_CONNECTION_STRING_NAME = "DefaultConnection";

    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = GetConnectionString(configuration);

        services.AddDbContext<MovieWatchlistDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IWatchlistRepository, WatchlistRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        return configuration[DATABASE_CONNECTION_STRING_ENV]
            ?? configuration.GetConnectionString(DEFAULT_CONNECTION_STRING_NAME)
            ?? throw new InvalidOperationException($"Database connection string '{DEFAULT_CONNECTION_STRING_NAME}' is required");
    }
}

