using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditedEntities.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameProductAuditTrailEntryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductAuditTrailEntries_Products_EntityId",
                table: "ProductAuditTrailEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductAuditTrailEntries",
                table: "ProductAuditTrailEntries");

            migrationBuilder.RenameTable(
                name: "ProductAuditTrailEntries",
                newName: "ProductAuditTrailEntry");

            migrationBuilder.RenameIndex(
                name: "IX_ProductAuditTrailEntries_EntityId",
                table: "ProductAuditTrailEntry",
                newName: "IX_ProductAuditTrailEntry_EntityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductAuditTrailEntry",
                table: "ProductAuditTrailEntry",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAuditTrailEntry_Products_EntityId",
                table: "ProductAuditTrailEntry",
                column: "EntityId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductAuditTrailEntry_Products_EntityId",
                table: "ProductAuditTrailEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductAuditTrailEntry",
                table: "ProductAuditTrailEntry");

            migrationBuilder.RenameTable(
                name: "ProductAuditTrailEntry",
                newName: "ProductAuditTrailEntries");

            migrationBuilder.RenameIndex(
                name: "IX_ProductAuditTrailEntry_EntityId",
                table: "ProductAuditTrailEntries",
                newName: "IX_ProductAuditTrailEntries_EntityId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductAuditTrailEntries",
                table: "ProductAuditTrailEntries",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductAuditTrailEntries_Products_EntityId",
                table: "ProductAuditTrailEntries",
                column: "EntityId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
