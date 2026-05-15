using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicPlayerDXMonoGamePort.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUpvotedSongAddingDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddingDates",
                table: "UpvotedSongs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AddingDates",
                table: "UpvotedSongs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
