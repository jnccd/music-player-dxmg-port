using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicPlayerDXMonoGamePort.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UpvotedSongs",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Score = table.Column<float>(type: "REAL", nullable: false),
                    Streak = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalLikes = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalDislikes = table.Column<int>(type: "INTEGER", nullable: false),
                    AddingDates = table.Column<long>(type: "INTEGER", nullable: false),
                    Volume = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpvotedSongs", x => x.Name);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UpvotedSongs");
        }
    }
}
