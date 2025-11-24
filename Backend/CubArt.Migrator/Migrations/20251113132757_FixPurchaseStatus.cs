using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CubArt.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class FixPurchaseStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PurchaseStatus",
                table: "purchase",
                newName: "purchase_status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "purchase_status",
                table: "purchase",
                newName: "PurchaseStatus");
        }
    }
}
