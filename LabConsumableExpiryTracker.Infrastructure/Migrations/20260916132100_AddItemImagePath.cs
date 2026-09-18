using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConsumableExpiryTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddItemImagePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "items",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "items");
        }
    }
}
