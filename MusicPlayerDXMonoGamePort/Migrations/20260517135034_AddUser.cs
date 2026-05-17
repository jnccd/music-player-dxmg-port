using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicPlayerDXMonoGamePort.Migrations
{
    /// <inheritdoc />
    public partial class AddUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SongHistoryEntries_UpvotedSongs_SongId",
                table: "SongHistoryEntries");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    UserHandle = table.Column<string>(type: "TEXT", nullable: false),
                    UserDisplayName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                });

            migrationBuilder.Sql(
                @"INSERT OR IGNORE INTO ""User"" (""UserId"", ""UserHandle"", ""UserDisplayName"")
                VALUES ('', 'dummy_user', 'Dummy User');");

            migrationBuilder.AddForeignKey(
                name: "FK_SongHistoryEntries_UpvotedSongs_SongId",
                table: "SongHistoryEntries",
                column: "SongId",
                principalTable: "UpvotedSongs",
                principalColumn: "SongId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UpvotedSongs_User_UserId",
                table: "UpvotedSongs",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SongHistoryEntries_UpvotedSongs_SongId",
                table: "SongHistoryEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_UpvotedSongs_User_UserId",
                table: "UpvotedSongs");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.AddForeignKey(
                name: "FK_SongHistoryEntries_UpvotedSongs_SongId",
                table: "SongHistoryEntries",
                column: "SongId",
                principalTable: "UpvotedSongs",
                principalColumn: "SongId");
        }
    }
}
