using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CubArt.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "facility",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facility", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    product_type = table.Column<int>(type: "integer", nullable: false),
                    unit_of_measure = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "supplier",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_specification",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_specification", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_specification_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "stock_balance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    facility_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    product_type = table.Column<int>(type: "integer", nullable: false),
                    operation_type = table.Column<int>(type: "integer", nullable: false),
                    start_balance = table.Column<decimal>(type: "numeric", nullable: false),
                    income_balance = table.Column<decimal>(type: "numeric", nullable: false),
                    outcome_balance = table.Column<decimal>(type: "numeric", nullable: false),
                    finish_balance = table.Column<decimal>(type: "numeric", nullable: false),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_balance", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_balance_facility_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facility",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_stock_balance_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "stock_movement",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    facility_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    operation_type = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_movement", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_movement_facility_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facility",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_stock_movement_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "purchase",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    supplier_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    facility_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchase_facility_facility_id",
                        column: x => x.facility_id,
                        principalTable: "facility",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_purchase_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_purchase_supplier_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "supplier",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "product_specification_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    product_specification_id = table.Column<int>(type: "integer", nullable: false),
                    product_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_specification_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_specification_item_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_product_specification_item_product_specification_product_sp~",
                        column: x => x.product_specification_id,
                        principalTable: "product_specification",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "idx_product_specification_product_id",
                table: "product_specification",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "idx_product_specification_item_product_specification_id",
                table: "product_specification_item",
                column: "product_specification_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_specification_item_product_id",
                table: "product_specification_item",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "idx_purchase_date_created",
                table: "purchase",
                column: "date_created");

            migrationBuilder.CreateIndex(
                name: "idx_purchase_facility_id",
                table: "purchase",
                column: "facility_id");

            migrationBuilder.CreateIndex(
                name: "idx_purchase_product_id",
                table: "purchase",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "idx_purchase_supplier_id",
                table: "purchase",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "idx_stock_balance_date_created",
                table: "stock_balance",
                column: "date_created");

            migrationBuilder.CreateIndex(
                name: "idx_stock_balance_facility_id",
                table: "stock_balance",
                column: "facility_id");

            migrationBuilder.CreateIndex(
                name: "idx_stock_balance_operation_type",
                table: "stock_balance",
                column: "operation_type");

            migrationBuilder.CreateIndex(
                name: "idx_stock_balance_product_id",
                table: "stock_balance",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "idx_stock_balance_product_type",
                table: "stock_balance",
                column: "product_type");

            migrationBuilder.CreateIndex(
                name: "idx_stock_movement_date_created",
                table: "stock_movement",
                column: "date_created");

            migrationBuilder.CreateIndex(
                name: "idx_stock_movement_facility_id",
                table: "stock_movement",
                column: "facility_id");

            migrationBuilder.CreateIndex(
                name: "idx_stock_movement_operation_type",
                table: "stock_movement",
                column: "operation_type");

            migrationBuilder.CreateIndex(
                name: "idx_stock_movement_product_id",
                table: "stock_movement",
                column: "product_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_specification_item");

            migrationBuilder.DropTable(
                name: "purchase");

            migrationBuilder.DropTable(
                name: "stock_balance");

            migrationBuilder.DropTable(
                name: "stock_movement");

            migrationBuilder.DropTable(
                name: "product_specification");

            migrationBuilder.DropTable(
                name: "supplier");

            migrationBuilder.DropTable(
                name: "facility");

            migrationBuilder.DropTable(
                name: "product");
        }
    }
}
