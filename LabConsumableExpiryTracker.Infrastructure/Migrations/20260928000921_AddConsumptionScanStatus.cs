using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConsumableExpiryTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConsumptionScanStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Kolom ditambahkan nullable dulu supaya baris lama bisa di-backfill
            // sebagai Verified: sebelum F-15 semua consumption single-phase dan
            // sudah otomatis mengurangi stok. Default "" akan menulis status
            // yang bukan nilai enum mana pun.
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "consumptions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "verified_at",
                table: "consumptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE consumptions
                SET status = 'Verified',
                    verified_at = COALESCE(consumed_at, verified_at);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "consumptions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_consumptions_pending",
                table: "consumptions",
                columns: new[] { "job_id", "lot_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_consumptions_pending",
                table: "consumptions");

            migrationBuilder.DropColumn(
                name: "status",
                table: "consumptions");

            migrationBuilder.DropColumn(
                name: "verified_at",
                table: "consumptions");
        }
    }
}
