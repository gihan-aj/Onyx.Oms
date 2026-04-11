using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Onyx.Oms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFulFillmentTasksTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FulFillmentTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedQuantity = table.Column<int>(type: "int", nullable: false),
                    StartedQuantity = table.Column<int>(type: "int", nullable: false),
                    CompletedQuantity = table.Column<int>(type: "int", nullable: false),
                    ScrappedQuantity = table.Column<int>(type: "int", nullable: false),
                    CostAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CostCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    AssignedUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PurchaseOrderNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LinkedOrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExpectedCompletionDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastModifiedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FulFillmentTasks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FulFillmentTasks_ProductVariantId",
                table: "FulFillmentTasks",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_FulFillmentTasks_TenantId",
                table: "FulFillmentTasks",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FulFillmentTasks");
        }
    }
}
