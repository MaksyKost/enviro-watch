using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnviroWatch.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManualObservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManualObservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Region = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Metric = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    Unit = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    Lat = table.Column<double>(type: "double precision", nullable: true),
                    Lon = table.Column<double>(type: "double precision", nullable: true),
                    Notes = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ObservedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManualObservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManualObservations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManualObservations_Region_Metric_ObservedAt",
                table: "ManualObservations",
                columns: new[] { "Region", "Metric", "ObservedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ManualObservations_UserId_ObservedAt",
                table: "ManualObservations",
                columns: new[] { "UserId", "ObservedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManualObservations");
        }
    }
}
