using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnviroWatch.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Metric = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Region = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Threshold = table.Column<double>(type: "double precision", nullable: false),
                    Condition = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    NotifyEmail = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastTriggeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlertLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    Threshold = table.Column<double>(type: "double precision", nullable: false),
                    TriggeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EmailSent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlertLogs_Alerts_AlertId",
                        column: x => x.AlertId,
                        principalTable: "Alerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_Region_Metric_IsActive",
                table: "Alerts",
                columns: new[] { "Region", "Metric", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_UserId_IsActive",
                table: "Alerts",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_AlertLogs_AlertId_TriggeredAt",
                table: "AlertLogs",
                columns: new[] { "AlertId", "TriggeredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertLogs");

            migrationBuilder.DropTable(
                name: "Alerts");
        }
    }
}
