using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_StockItems_StockItemId",
                table: "Reservations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockItems",
                table: "StockItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reservations",
                table: "Reservations");

            migrationBuilder.RenameTable(
                name: "StockItems",
                newName: "stock_item");

            migrationBuilder.RenameTable(
                name: "Reservations",
                newName: "reservation");

            migrationBuilder.RenameIndex(
                name: "IX_Reservations_StockItemId",
                table: "reservation",
                newName: "IX_reservation_StockItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_stock_item",
                table: "stock_item",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_reservation",
                table: "reservation",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_reservation_stock_item_StockItemId",
                table: "reservation",
                column: "StockItemId",
                principalTable: "stock_item",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reservation_stock_item_StockItemId",
                table: "reservation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_stock_item",
                table: "stock_item");

            migrationBuilder.DropPrimaryKey(
                name: "PK_reservation",
                table: "reservation");

            migrationBuilder.RenameTable(
                name: "stock_item",
                newName: "StockItems");

            migrationBuilder.RenameTable(
                name: "reservation",
                newName: "Reservations");

            migrationBuilder.RenameIndex(
                name: "IX_reservation_StockItemId",
                table: "Reservations",
                newName: "IX_Reservations_StockItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockItems",
                table: "StockItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reservations",
                table: "Reservations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_StockItems_StockItemId",
                table: "Reservations",
                column: "StockItemId",
                principalTable: "StockItems",
                principalColumn: "Id");
        }
    }
}
