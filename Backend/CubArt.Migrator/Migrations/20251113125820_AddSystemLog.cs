using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CubArt.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PurchaseStatus",
                table: "purchase",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "system_log",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    request_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    request_method = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    exception_type = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    stack_trace = table.Column<string>(type: "text", nullable: true),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    entity_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    additional_data = table.Column<string>(type: "jsonb", nullable: true),
                    date_created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'utc'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_log", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_product_name",
                table: "product",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_product_product_type",
                table: "product",
                column: "product_type");

            migrationBuilder.CreateIndex(
                name: "idx_system_log_date_created",
                table: "system_log",
                column: "date_created");

            migrationBuilder.CreateIndex(
                name: "idx_system_log_level",
                table: "system_log",
                column: "level");

            migrationBuilder.CreateIndex(
                name: "idx_system_log_source",
                table: "system_log",
                column: "source");

            migrationBuilder.CreateIndex(
                name: "idx_system_log_user_id",
                table: "system_log",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "system_log");

            migrationBuilder.DropIndex(
                name: "idx_product_name",
                table: "product");

            migrationBuilder.DropIndex(
                name: "idx_product_product_type",
                table: "product");

            migrationBuilder.DropColumn(
                name: "PurchaseStatus",
                table: "purchase");
        }
    }
}
