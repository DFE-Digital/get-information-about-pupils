using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class IngestTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ingest_files",
                columns: table => new
                {
                    path = table.Column<string>(type: "text", nullable: false),
                    blob_name = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    error = table.Column<string>(type: "text", nullable: true),
                    attempts = table.Column<int>(type: "integer", nullable: false),
                    first_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ingest_files", x => x.path);
                });

            migrationBuilder.CreateTable(
                name: "ingest_runs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    attempt = table.Column<int>(type: "integer", nullable: false),
                    data_file_name = table.Column<string>(type: "text", nullable: false),
                    dataset = table.Column<string>(type: "text", nullable: true),
                    academic_year = table.Column<string>(type: "text", nullable: true),
                    ingest_version = table.Column<short>(type: "smallint", nullable: true),
                    records_read = table.Column<int>(type: "integer", nullable: true),
                    records_written = table.Column<int>(type: "integer", nullable: true),
                    records_removed = table.Column<int>(type: "integer", nullable: true),
                    records_rejected = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    error = table.Column<string>(type: "text", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ingest_runs", x => x.id);
                    table.ForeignKey(
                        name: "fk_ingest_runs_ingest_files_file_path",
                        column: x => x.file_path,
                        principalTable: "ingest_files",
                        principalColumn: "path",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ingest_rejected_rows",
                columns: table => new
                {
                    run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    row_number = table.Column<int>(type: "integer", nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false),
                    reasons = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ingest_rejected_rows", x => new { x.run_id, x.row_number });
                    table.ForeignKey(
                        name: "fk_ingest_rejected_rows_ingest_runs_run_id",
                        column: x => x.run_id,
                        principalTable: "ingest_runs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ingest_files_status",
                table: "ingest_files",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_ingest_runs_file_path_attempt",
                table: "ingest_runs",
                columns: new[] { "file_path", "attempt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ingest_rejected_rows");

            migrationBuilder.DropTable(
                name: "ingest_runs");

            migrationBuilder.DropTable(
                name: "ingest_files");
        }
    }
}
