using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieWatchlist.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddValueObjectsSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserRating",
                table: "WatchlistItems",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "User's personal rating (1-10)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "User's personal rating (1-10)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UserRating",
                table: "WatchlistItems",
                type: "integer",
                nullable: true,
                comment: "User's personal rating (1-10)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldComment: "User's personal rating (1-10)");
        }
    }
}

