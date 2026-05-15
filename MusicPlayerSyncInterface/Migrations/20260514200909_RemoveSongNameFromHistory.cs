using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicPlayerDXMonoGamePort.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSongNameFromHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SongHistoryEntries_UpvotedSongs_SongId",
                table: "SongHistoryEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SongHistoryEntries",
                table: "SongHistoryEntries");

            migrationBuilder.DropColumn(
                name: "SongName",
                table: "SongHistoryEntries");

            migrationBuilder.AlterColumn<Guid>(
                name: "SongId",
                table: "SongHistoryEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SongHistoryEntries",
                table: "SongHistoryEntries",
                columns: new[] { "UserId", "SongId", "Date" });

            migrationBuilder.AddForeignKey(
                name: "FK_SongHistoryEntries_UpvotedSongs_SongId",
                table: "SongHistoryEntries",
                column: "SongId",
                principalTable: "UpvotedSongs",
                principalColumn: "SongId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SongHistoryEntries_UpvotedSongs_SongId",
                table: "SongHistoryEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SongHistoryEntries",
                table: "SongHistoryEntries");

            migrationBuilder.AlterColumn<Guid>(
                name: "SongId",
                table: "SongHistoryEntries",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "SongName",
                table: "SongHistoryEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SongHistoryEntries",
                table: "SongHistoryEntries",
                columns: new[] { "UserId", "SongName", "Date" });

            migrationBuilder.AddForeignKey(
                name: "FK_SongHistoryEntries_UpvotedSongs_SongId",
                table: "SongHistoryEntries",
                column: "SongId",
                principalTable: "UpvotedSongs",
                principalColumn: "SongId");
        }
    }
}
