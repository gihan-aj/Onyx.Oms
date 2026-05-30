using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onyx.Oms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTimestampsForOrdersAndProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OutOfStockSinceUtc",
                table: "ProductVariants",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAtUtc",
                table: "Orders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeliveredAtUtc",
                table: "Orders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PackedAtUtc",
                table: "Orders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ShippedAtUtc",
                table: "Orders",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutOfStockSinceUtc",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "CancelledAtUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveredAtUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PackedAtUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippedAtUtc",
                table: "Orders");
        }
    }
}
