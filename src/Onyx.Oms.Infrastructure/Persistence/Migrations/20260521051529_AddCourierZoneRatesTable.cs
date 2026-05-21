using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onyx.Oms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCourierZoneRatesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourierZoneRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ZoneName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BaseWeightValue = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: false),
                    BaseWeightUnit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CodPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CoveredDistrics = table.Column<string>(type: "json", nullable: false),
                    BaseFeeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BaseFeeCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExcessFeePerWeightUnitAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExcessFeePerWeightUnitCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourierZoneRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourierZoneRates_Couriers_CourierId",
                        column: x => x.CourierId,
                        principalTable: "Couriers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourierZoneRates_CourierId",
                table: "CourierZoneRates",
                column: "CourierId");

            migrationBuilder.CreateIndex(
                name: "IX_CourierZoneRates_TenantId",
                table: "CourierZoneRates",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourierZoneRates");
        }
    }
}
