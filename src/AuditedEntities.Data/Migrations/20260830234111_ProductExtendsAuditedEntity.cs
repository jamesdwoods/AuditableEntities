using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditedEntities.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProductExtendsAuditedEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "ProductAuditTrailEntries",
                type: "nvarchar(34)",
                maxLength: 34,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "ProductAuditTrailEntries");
        }
    }
}
