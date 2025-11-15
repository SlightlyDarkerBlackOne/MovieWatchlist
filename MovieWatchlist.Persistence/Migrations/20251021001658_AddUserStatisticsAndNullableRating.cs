using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieWatchlist.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStatisticsAndNullableRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
    }
}

