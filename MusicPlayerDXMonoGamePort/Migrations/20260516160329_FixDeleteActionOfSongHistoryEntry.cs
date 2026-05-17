using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicPlayerDXMonoGamePort.Migrations
{
    /// <inheritdoc />
    public partial class FixDeleteActionOfSongHistoryEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SongHistoryEntries_UpvotedSongs_SongId",
                table: "SongHistoryEntries");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UpvotedSongs",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UpvotedSongs",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Artist",
                table: "UpvotedSongs",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Album",
                table: "UpvotedSongs",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

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

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UpvotedSongs",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UpvotedSongs",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Artist",
                table: "UpvotedSongs",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Album",
                table: "UpvotedSongs",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_SongHistoryEntries_UpvotedSongs_SongId",
                table: "SongHistoryEntries",
                column: "SongId",
                principalTable: "UpvotedSongs",
                principalColumn: "SongId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
