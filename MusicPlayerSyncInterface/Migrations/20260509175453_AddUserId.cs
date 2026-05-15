using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicPlayerDXMonoGamePort.Migrations
{
    /// <inheritdoc />
    public partial class AddUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UpvotedSongs",
                table: "UpvotedSongs");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "UpvotedSongs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UpvotedSongs",
                table: "UpvotedSongs",
                columns: new[] { "UserId", "Name", "Artist", "Album" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UpvotedSongs",
                table: "UpvotedSongs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UpvotedSongs");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UpvotedSongs",
                table: "UpvotedSongs",
                columns: new[] { "Name", "Artist", "Album" });
        }
    }
}
