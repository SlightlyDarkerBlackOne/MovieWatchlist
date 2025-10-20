using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Core.Models;
using MovieWatchlist.Core.ValueObjects;

namespace MovieWatchlist.Infrastructure.Data;

public class MovieWatchlistDbContext : DbContext
{
    public MovieWatchlistDbContext(DbContextOptions<MovieWatchlistDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<WatchlistItem> WatchlistItems { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure each entity separately for better readability
        ConfigureUserEntity(modelBuilder);
        ConfigureMovieEntity(modelBuilder);
        ConfigureWatchlistItemEntity(modelBuilder);
        ConfigureRefreshTokenEntity(modelBuilder);
        ConfigurePasswordResetTokenEntity(modelBuilder);
    }

    /// <summary>
    /// Configures the User entity with properties, indexes, and relationships
    /// </summary>
    private static void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);

            // Property configurations
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion(
                    v => v.Value,
                    v => Username.Create(v).Value!)
                .HasComment("Unique username for the user");
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasConversion(
                    v => v.Value,
                    v => Email.Create(v).Value!)
                .HasComment("User's email address");
            
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(255)
                .HasComment("Hashed password using PBKDF2");
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("When the user account was created");
            
            entity.Property(e => e.LastLoginAt)
                .HasComment("Last successful login timestamp");
            
            entity.Property(e => e.CachedStatisticsJson)
                .HasColumnType("jsonb")
                .HasComment("Cached watchlist statistics as JSONB");
            
            entity.Property(e => e.StatisticsLastUpdated)
                .HasComment("When statistics were last calculated");

            // Indexes for performance
            entity.HasIndex(e => e.Username)
                .IsUnique()
                .HasDatabaseName("IX_Users_Username");
            
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");
            
            entity.HasIndex(e => e.StatisticsLastUpdated)
                .HasDatabaseName("IX_Users_StatisticsLastUpdated");

            // Relationships
            entity.HasMany(e => e.WatchlistItems)
                .WithOne()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Configures the Movie entity with properties, indexes, and relationships
    /// </summary>
    private static void ConfigureMovieEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);
            
            // Property configurations
            entity.Property(e => e.TmdbId)
                .IsRequired()
                .HasComment("The Movie Database ID");
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500)
                .HasComment("Movie title");
            
            entity.Property(e => e.Overview)
                .HasMaxLength(5000)
                .HasComment("Movie plot summary");
            
            entity.Property(e => e.PosterPath)
                .HasMaxLength(500)
                .HasComment("Path to movie poster image");
            
            entity.Property(e => e.ReleaseDate)
                .IsRequired()
                .HasComment("Movie release date");
            
            entity.Property(e => e.VoteAverage)
                .HasColumnType("decimal(3,1)")
                .HasComment("Average user rating from TMDB");
            
            entity.Property(e => e.VoteCount)
                .HasComment("Number of votes on TMDB");
            
            entity.Property(e => e.Popularity)
                .HasColumnType("decimal(10,2)")
                .HasComment("Popularity score from TMDB");
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("When this movie was added to our database");
            
            entity.Property(e => e.UpdatedAt)
                .HasComment("Last time movie data was updated");

            // Configure Genres as JSON array for PostgreSQL
            entity.Property(e => e.Genres)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<string[]>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? Array.Empty<string>())
                .HasComment("Movie genres as JSON array");

            // Indexes for performance
            entity.HasIndex(e => e.TmdbId)
                .IsUnique()
                .HasDatabaseName("IX_Movies_TmdbId");
            
            entity.HasIndex(e => e.Title)
                .HasDatabaseName("IX_Movies_Title");
            
            entity.HasIndex(e => e.ReleaseDate)
                .HasDatabaseName("IX_Movies_ReleaseDate");

            // Relationships
            entity.HasMany<WatchlistItem>()
                .WithOne(e => e.Movie)
                .HasForeignKey(e => e.MovieId)
                .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete movies when watchlist items are deleted
        });
    }

    /// <summary>
    /// Configures the WatchlistItem entity with properties, constraints, indexes, and relationships
    /// </summary>
    private static void ConfigureWatchlistItemEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WatchlistItem>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);
            
            // Property configurations
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasComment("Reference to the user who owns this watchlist item");
            
            entity.Property(e => e.MovieId)
                .IsRequired()
                .HasComment("Reference to the movie (cached from TMDB)");
            
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20)
                .HasComment("Current status of the movie in user's watchlist");
            
            entity.Property(e => e.IsFavorite)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Whether this movie is marked as favorite");
            
            entity.Property(e => e.UserRating)
                .HasConversion(
                    v => v != null ? v.Value : (int?)null,
                    v => v.HasValue ? Rating.Create(v.Value).Value! : null!)
                .HasComment("User's personal rating (1-10)");
            
            entity.Property(e => e.Notes)
                .HasMaxLength(1000)
                .HasComment("User's personal notes about the movie");
            
            entity.Property(e => e.AddedDate)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("When this movie was added to the watchlist");
            
            entity.Property(e => e.WatchedDate)
                .HasComment("When the user marked this movie as watched");

            // Database constraints
            entity.ToTable(t => t.HasCheckConstraint("CK_WatchlistItems_UserRating", 
                "\"UserRating\" IS NULL OR (\"UserRating\" >= 1 AND \"UserRating\" <= 10)"));

            // Indexes for performance
            entity.HasIndex(e => new { e.UserId, e.MovieId })
                .IsUnique()
                .HasDatabaseName("IX_WatchlistItems_UserId_MovieId");
            
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_WatchlistItems_UserId");
            
            entity.HasIndex(e => e.MovieId)
                .HasDatabaseName("IX_WatchlistItems_MovieId");
            
            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_WatchlistItems_Status");
            
            entity.HasIndex(e => e.AddedDate)
                .HasDatabaseName("IX_WatchlistItems_AddedDate");

            // Relationships
            entity.HasOne(e => e.Movie)
                .WithMany()
                .HasForeignKey(e => e.MovieId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    /// <summary>
    /// Configures the RefreshToken entity with properties, indexes, and relationships
    /// </summary>
    private static void ConfigureRefreshTokenEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            // Primary key
            entity.HasKey(e => e.Id);
            
            // Property configurations
            entity.Property(e => e.UserId)
                .IsRequired()
                .HasComment("Reference to the user who owns this refresh token");
            
            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(500)
                .HasComment("The refresh token string");
            
            entity.Property(e => e.ExpiresAt)
                .IsRequired()
                .HasComment("When this refresh token expires");
            
            entity.Property(e => e.IsRevoked)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Whether this token has been revoked");
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("When this token was created");

            // Indexes for performance
            entity.HasIndex(e => e.Token)
                .IsUnique()
                .HasDatabaseName("IX_RefreshTokens_Token");
            
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_RefreshTokens_UserId");
            
            entity.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("IX_RefreshTokens_ExpiresAt");

            // Relationships
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Configures the PasswordResetToken entity with properties, indexes, and relationships
    /// </summary>
    private static void ConfigurePasswordResetTokenEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.ExpiresAt)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.IsUsed)
                .IsRequired()
                .HasDefaultValue(false);

            // Indexes for performance
            entity.HasIndex(e => e.Token)
                .IsUnique();

            entity.HasIndex(e => new { e.UserId, e.IsUsed });

            entity.HasIndex(e => e.ExpiresAt);

            // Relationship with User
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

