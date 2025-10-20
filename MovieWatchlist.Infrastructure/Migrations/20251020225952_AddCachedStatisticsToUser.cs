using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieWatchlist.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCachedStatisticsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CachedStatisticsJson",
                table: "Users",
                type: "jsonb",
                nullable: true,
                comment: "Cached watchlist statistics as JSONB");

            migrationBuilder.AddColumn<DateTime>(
                name: "StatisticsLastUpdated",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true,
                comment: "When statistics were last calculated");

            migrationBuilder.CreateIndex(
                name: "IX_Users_StatisticsLastUpdated",
                table: "Users",
                column: "StatisticsLastUpdated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_StatisticsLastUpdated",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CachedStatisticsJson",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StatisticsLastUpdated",
                table: "Users");
        }
    }
}
