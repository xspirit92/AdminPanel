using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CubArt.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddSupply : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_stock_balance_operation_type",
                table: "stock_balance");

            migrationBuilder.DropIndex(
                name: "idx_stock_balance_product_type",
                table: "stock_balance");

            migrationBuilder.DropColumn(
                name: "operation_type",
                table: "stock_balance");

            migrationBuilder.DropColumn(
                name: "product_type",
                table: "stock_balance");

            migrationBuilder.AddColumn<string>(
                name: "reference_id",
                table: "stock_movement",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "reference_type",
                table: "stock_movement",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "supply",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    purchase_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supply", x => x.id);
                    table.ForeignKey(
                        name: "FK_supply_purchase_purchase_id",
                        column: x => x.purchase_id,
                        principalTable: "purchase",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "idx_stock_movement_reference_id",
                table: "stock_movement",
                column: "reference_id");

            migrationBuilder.CreateIndex(
                name: "idx_stock_movement_reference_type",
                table: "stock_movement",
                column: "reference_type");

            migrationBuilder.CreateIndex(
                name: "idx_supply_date_created",
                table: "supply",
                column: "date_created");

            migrationBuilder.CreateIndex(
                name: "idx_supply_purchase_id",
                table: "supply",
                column: "purchase_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supply");

            migrationBuilder.DropIndex(
                name: "idx_stock_movement_reference_id",
                table: "stock_movement");

            migrationBuilder.DropIndex(
                name: "idx_stock_movement_reference_type",
                table: "stock_movement");

            migrationBuilder.DropColumn(
                name: "reference_id",
                table: "stock_movement");

            migrationBuilder.DropColumn(
                name: "reference_type",
                table: "stock_movement");

            migrationBuilder.AddColumn<int>(
                name: "operation_type",
                table: "stock_balance",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "product_type",
                table: "stock_balance",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "idx_stock_balance_operation_type",
                table: "stock_balance",
                column: "operation_type");

            migrationBuilder.CreateIndex(
                name: "idx_stock_balance_product_type",
                table: "stock_balance",
                column: "product_type");
        }
    }
}
