using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicPlayerDXMonoGamePort.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeyToSongHistoryEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SongId",
                table: "SongHistoryEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SongHistoryEntries_SongId",
                table: "SongHistoryEntries",
                column: "SongId");

            migrationBuilder.AddForeignKey(
                name: "FK_SongHistoryEntries_UpvotedSongs_SongId",
                table: "SongHistoryEntries",
                column: "SongId",
                principalTable: "UpvotedSongs",
                principalColumn: "SongId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SongHistoryEntries_UpvotedSongs_SongId",
                table: "SongHistoryEntries");

            migrationBuilder.DropIndex(
                name: "IX_SongHistoryEntries_SongId",
                table: "SongHistoryEntries");

            migrationBuilder.DropColumn(
                name: "SongId",
                table: "SongHistoryEntries");
        }
    }
}
