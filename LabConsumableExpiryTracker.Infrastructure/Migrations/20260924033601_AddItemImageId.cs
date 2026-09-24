using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConsumableExpiryTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddItemImageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "items");

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "items",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "items");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "items",
                type: "text",
                nullable: true);
        }
    }
}
