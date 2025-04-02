using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlotPocket.Server.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookmarksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShowUser_AspNetUsers_UsersId",
                table: "ShowUser");

            migrationBuilder.DropForeignKey(
                name: "FK_ShowUser_Shows_ShowsId",
                table: "ShowUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShowUser",
                table: "ShowUser");

            migrationBuilder.RenameTable(
                name: "ShowUser",
                newName: "UserShows");

            migrationBuilder.RenameIndex(
                name: "IX_ShowUser_UsersId",
                table: "UserShows",
                newName: "IX_UserShows_UsersId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserShows",
                table: "UserShows",
                columns: new[] { "ShowsId", "UsersId" });

            migrationBuilder.CreateTable(
                name: "Bookmarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ShowId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookmarks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_ShowId_UserId",
                table: "Bookmarks",
                columns: new[] { "ShowId", "UserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserShows_AspNetUsers_UsersId",
                table: "UserShows",
                column: "UsersId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserShows_Shows_ShowsId",
                table: "UserShows",
                column: "ShowsId",
                principalTable: "Shows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserShows_AspNetUsers_UsersId",
                table: "UserShows");

            migrationBuilder.DropForeignKey(
                name: "FK_UserShows_Shows_ShowsId",
                table: "UserShows");

            migrationBuilder.DropTable(
                name: "Bookmarks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserShows",
                table: "UserShows");

            migrationBuilder.RenameTable(
                name: "UserShows",
                newName: "ShowUser");

            migrationBuilder.RenameIndex(
                name: "IX_UserShows_UsersId",
                table: "ShowUser",
                newName: "IX_ShowUser_UsersId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShowUser",
                table: "ShowUser",
                columns: new[] { "ShowsId", "UsersId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ShowUser_AspNetUsers_UsersId",
                table: "ShowUser",
                column: "UsersId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShowUser_Shows_ShowsId",
                table: "ShowUser",
                column: "ShowsId",
                principalTable: "Shows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
