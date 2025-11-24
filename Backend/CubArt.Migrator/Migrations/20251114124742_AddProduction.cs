using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CubArt.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddProduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "production",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    facility_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production", x => x.id);
                    table.ForeignKey(
                        name: "FK_production_facility_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facility",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_production_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "idx_stock_balance_facility_id_product_id_date_created",
                table: "stock_balance",
                columns: new[] { "facility_id", "product_id", "date_created" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_production_date_created",
                table: "production",
                column: "date_created");

            migrationBuilder.CreateIndex(
                name: "idx_production_facility_id",
                table: "production",
                column: "facility_id");

            migrationBuilder.CreateIndex(
                name: "idx_production_product_id",
                table: "production",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "production");

            migrationBuilder.DropIndex(
                name: "idx_stock_balance_facility_id_product_id_date_created",
                table: "stock_balance");
        }
    }
}
