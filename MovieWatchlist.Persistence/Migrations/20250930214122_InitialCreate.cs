using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MovieWatchlist.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TmdbId = table.Column<int>(type: "integer", nullable: false, comment: "The Movie Database ID"),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "Movie title"),
                    Overview = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false, comment: "Movie plot summary"),
                    PosterPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "Path to movie poster image"),
                    ReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Movie release date"),
                    VoteAverage = table.Column<double>(type: "numeric(3,1)", nullable: false, comment: "Average user rating from TMDB"),
                    VoteCount = table.Column<int>(type: "integer", nullable: false, comment: "Number of votes on TMDB"),
                    Popularity = table.Column<double>(type: "numeric(10,2)", nullable: false, comment: "Popularity score from TMDB"),
                    Genres = table.Column<string>(type: "text", nullable: false, comment: "Movie genres as JSON array"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "When this movie was added to our database"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Last time movie data was updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "Unique username for the user"),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "User's email address"),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Hashed password using PBKDF2"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "When the user account was created"),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Last successful login timestamp")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false, comment: "Reference to the user who owns this refresh token"),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "The refresh token string"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "When this refresh token expires"),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Whether this token has been revoked"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "When this token was created")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WatchlistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false, comment: "Reference to the user who owns this watchlist item"),
                    MovieId = table.Column<int>(type: "integer", nullable: false, comment: "Reference to the movie"),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "Current status of the movie in user's watchlist"),
                    IsFavorite = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Whether this movie is marked as favorite"),
                    UserRating = table.Column<int>(type: "integer", nullable: true, comment: "User's personal rating (1-10)"),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "User's personal notes about the movie"),
                    AddedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "When this movie was added to the watchlist"),
                    WatchedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "When the user marked this movie as watched")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchlistItems", x => x.Id);
                    table.CheckConstraint("CK_WatchlistItems_UserRating", "\"UserRating\" IS NULL OR (\"UserRating\" >= 1 AND \"UserRating\" <= 10)");
                    table.ForeignKey(
                        name: "FK_WatchlistItems_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WatchlistItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movies_ReleaseDate",
                table: "Movies",
                column: "ReleaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_Title",
                table: "Movies",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Movies_TmdbId",
                table: "Movies",
                column: "TmdbId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistItems_AddedDate",
                table: "WatchlistItems",
                column: "AddedDate");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistItems_MovieId",
                table: "WatchlistItems",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistItems_Status",
                table: "WatchlistItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistItems_UserId",
                table: "WatchlistItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchlistItems_UserId_MovieId",
                table: "WatchlistItems",
                columns: new[] { "UserId", "MovieId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "WatchlistItems");

            migrationBuilder.DropTable(
                name: "Movies");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

