using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlacementDriveService.Migrations
{
    /// <inheritdoc />
    public partial class InititalCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    Key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlacementDrives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    JobRole = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Package = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AllowedBranches = table.Column<string>(type: "text", nullable: false),
                    DriveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApplicationDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlacementDrives", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_IsProcessed_CreatedAt",
                table: "OutboxMessages",
                columns: new[] { "IsProcessed", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PlacementDrives_CompanyName",
                table: "PlacementDrives",
                column: "CompanyName");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementDrives_DriveDate",
                table: "PlacementDrives",
                column: "DriveDate");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementDrives_Status",
                table: "PlacementDrives",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "PlacementDrives");
        }
    }
}
