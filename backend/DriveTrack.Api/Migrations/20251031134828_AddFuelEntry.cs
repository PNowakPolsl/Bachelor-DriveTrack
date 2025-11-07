using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DriveTrack.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFuelEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FuelEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    FuelTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Volume = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    OdometerKm = table.Column<int>(type: "integer", nullable: false),
                    IsFullTank = table.Column<bool>(type: "boolean", nullable: false),
                    Station = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FuelEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FuelEntries_FuelTypes_FuelTypeId",
                        column: x => x.FuelTypeId,
                        principalTable: "FuelTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FuelEntries_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FuelEntries_FuelTypeId",
                table: "FuelEntries",
                column: "FuelTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FuelEntries_VehicleId_Date",
                table: "FuelEntries",
                columns: new[] { "VehicleId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_FuelEntries_VehicleId_OdometerKm",
                table: "FuelEntries",
                columns: new[] { "VehicleId", "OdometerKm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FuelEntries");
        }
    }
}
