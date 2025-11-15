using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MovieWatchlist.Persistence.Data;

/// <summary>
/// Design-time factory for Entity Framework migrations
/// </summary>
public class MovieWatchlistDbContextFactory : IDesignTimeDbContextFactory<MovieWatchlistDbContext>
{
    public MovieWatchlistDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MovieWatchlistDbContext>();
        
        // Get connection string from environment variable or use default
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
            ?? "Host=localhost;Database=MovieWatchlistDb;Username=postgres;Password=password;Port=5432";
        
        optionsBuilder.UseNpgsql(connectionString);
        
        return new MovieWatchlistDbContext(optionsBuilder.Options);
    }
}

