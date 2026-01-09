using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Migrations
{
    /// <inheritdoc />
    public partial class FixUserRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UserId1",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTokens_Users_UserId1",
                table: "UserTokens");

            migrationBuilder.DropIndex(
                name: "IX_UserTokens_UserId1",
                table: "UserTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId1",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserTokens");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "RefreshTokens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "UserTokens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "RefreshTokens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UserId1",
                table: "UserTokens",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId1",
                table: "RefreshTokens",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UserId1",
                table: "RefreshTokens",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTokens_Users_UserId1",
                table: "UserTokens",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
