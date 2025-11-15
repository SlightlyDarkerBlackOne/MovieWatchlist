using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieWatchlist.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditsAndVideosToMovie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MovieId",
                table: "WatchlistItems",
                type: "integer",
                nullable: false,
                comment: "Reference to the movie (cached from TMDB)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Reference to the movie");

            migrationBuilder.AddColumn<string>(
                name: "BackdropPath",
                table: "Movies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditsJson",
                table: "Movies",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VideosJson",
                table: "Movies",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackdropPath",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "CreditsJson",
                table: "Movies");

            migrationBuilder.DropColumn(
                name: "VideosJson",
                table: "Movies");

            migrationBuilder.AlterColumn<int>(
                name: "MovieId",
                table: "WatchlistItems",
                type: "integer",
                nullable: false,
                comment: "Reference to the movie",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "Reference to the movie (cached from TMDB)");
        }
    }
}

