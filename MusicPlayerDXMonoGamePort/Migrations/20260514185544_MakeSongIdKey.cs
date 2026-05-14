using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicPlayerDXMonoGamePort.Migrations
{
    /// <inheritdoc />
    public partial class MakeSongIdKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UpvotedSongs",
                table: "UpvotedSongs");

            migrationBuilder.AlterColumn<Guid>(
                name: "SongId",
                table: "UpvotedSongs",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Album",
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
                name: "Name",
                table: "UpvotedSongs",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UpvotedSongs",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UpvotedSongs",
                table: "UpvotedSongs",
                column: "SongId");

            migrationBuilder.CreateIndex(
                name: "IX_UpvotedSongs_UserId_Name_Artist_Album",
                table: "UpvotedSongs",
                columns: new[] { "UserId", "Name", "Artist", "Album" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UpvotedSongs",
                table: "UpvotedSongs");

            migrationBuilder.DropIndex(
                name: "IX_UpvotedSongs_UserId_Name_Artist_Album",
                table: "UpvotedSongs");

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

            migrationBuilder.AlterColumn<Guid>(
                name: "SongId",
                table: "UpvotedSongs",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UpvotedSongs",
                table: "UpvotedSongs",
                columns: new[] { "UserId", "Name", "Artist", "Album" });
        }
    }
}
