using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConsumableExpiryTracker.Infrastructure.Migrations
{
    public partial class AddJobDescriptionAndAssignedScientist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "jobs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "assigned_scientist_id",
                table: "jobs",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "assigned_scientist_id",
                table: "jobs");
        }
    
    }
}
