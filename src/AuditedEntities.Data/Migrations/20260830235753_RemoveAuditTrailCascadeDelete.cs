using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditedEntities.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAuditTrailCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductAuditTrailEntries_Products_EntityId",
                table: "ProductAuditTrailEntries");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAuditTrailEntries_Products_EntityId",
                table: "ProductAuditTrailEntries",
                column: "EntityId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductAuditTrailEntries_Products_EntityId",
                table: "ProductAuditTrailEntries");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAuditTrailEntries_Products_EntityId",
                table: "ProductAuditTrailEntries",
                column: "EntityId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
