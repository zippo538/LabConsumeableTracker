using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabConsumableExpiryTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    base_unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    minimum_stock = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    expiring_soon_days = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.id);
                    table.CheckConstraint("ck_items_expiring_soon_days", "\"expiring_soon_days\" >= 0");
                    table.CheckConstraint("ck_items_minimum_stock", "\"minimum_stock\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jobs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lot_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    supplier_lot_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    supplier_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    initial_quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    remaining_quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    expiry_date = table.Column<DateOnly>(type: "date", nullable: false),
                    storage_location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lots", x => x.id);
                    table.CheckConstraint("ck_lots_initial_quantity", "\"initial_quantity\" >= 0");
                    table.CheckConstraint("ck_lots_remaining_not_above_initial", "\"remaining_quantity\" <= \"initial_quantity\"");
                    table.CheckConstraint("ck_lots_remaining_quantity", "\"remaining_quantity\" >= 0");
                    table.ForeignKey(
                        name: "FK_lots_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "consumptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    consumed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    consumed_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consumptions", x => x.id);
                    table.CheckConstraint("ck_consumptions_quantity", "\"quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_consumptions_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_consumptions_lots_lot_id",
                        column: x => x.lot_id,
                        principalTable: "lots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "disposals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    disposed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    disposed_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_disposals", x => x.id);
                    table.CheckConstraint("ck_disposals_quantity", "\"quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_disposals_lots_lot_id",
                        column: x => x.lot_id,
                        principalTable: "lots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_consumptions_job_id",
                table: "consumptions",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "ix_consumptions_lot_id",
                table: "consumptions",
                column: "lot_id");

            migrationBuilder.CreateIndex(
                name: "ix_disposals_lot_id",
                table: "disposals",
                column: "lot_id");

            migrationBuilder.CreateIndex(
                name: "ux_items_code",
                table: "items",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_jobs_job_number",
                table: "jobs",
                column: "job_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lots_fefo",
                table: "lots",
                columns: new[] { "item_id", "status", "expiry_date", "received_at" });

            migrationBuilder.CreateIndex(
                name: "ux_lots_item_id_lot_number",
                table: "lots",
                columns: new[] { "item_id", "lot_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consumptions");

            migrationBuilder.DropTable(
                name: "disposals");

            migrationBuilder.DropTable(
                name: "jobs");

            migrationBuilder.DropTable(
                name: "lots");

            migrationBuilder.DropTable(
                name: "items");
        }
    }
}
