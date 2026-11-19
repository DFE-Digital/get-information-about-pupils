using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PupilData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "further_education_learners",
                columns: table => new
                {
                    uln = table.Column<string>(type: "text", nullable: false),
                    surname = table.Column<string>(type: "text", nullable: true),
                    forename = table.Column<string>(type: "text", nullable: true),
                    sex = table.Column<string>(type: "text", nullable: true),
                    dob = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_further_education_learners", x => x.uln);
                });

            migrationBuilder.CreateTable(
                name: "npd_pupils",
                columns: table => new
                {
                    pupil_matching_ref = table.Column<string>(type: "text", nullable: false),
                    upn = table.Column<string>(type: "text", nullable: true),
                    la_code = table.Column<int>(type: "integer", nullable: true),
                    establishment_number = table.Column<int>(type: "integer", nullable: true),
                    urn = table.Column<int>(type: "integer", nullable: true),
                    surname = table.Column<string>(type: "text", nullable: true),
                    forename = table.Column<string>(type: "text", nullable: true),
                    middle_names = table.Column<string>(type: "text", nullable: true),
                    sex = table.Column<string>(type: "text", nullable: true),
                    dob = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_npd_pupils", x => x.pupil_matching_ref);
                });

            migrationBuilder.CreateTable(
                name: "pupil_premium_pupils",
                columns: table => new
                {
                    pupil_matching_ref = table.Column<string>(type: "text", nullable: false),
                    upn = table.Column<string>(type: "text", nullable: true),
                    la_code = table.Column<int>(type: "integer", nullable: true),
                    establishment_number = table.Column<int>(type: "integer", nullable: true),
                    urn = table.Column<int>(type: "integer", nullable: true),
                    surname = table.Column<string>(type: "text", nullable: true),
                    forename = table.Column<string>(type: "text", nullable: true),
                    middle_names = table.Column<string>(type: "text", nullable: true),
                    sex = table.Column<string>(type: "text", nullable: true),
                    dob = table.Column<DateOnly>(type: "date", nullable: true),
                    source_flag = table.Column<string>(type: "text", nullable: true),
                    data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pupil_premium_pupils", x => x.pupil_matching_ref);
                });

            migrationBuilder.CreateTable(
                name: "further_education_learner_records",
                columns: table => new
                {
                    uln = table.Column<string>(type: "text", nullable: false),
                    dataset = table.Column<string>(type: "text", nullable: false),
                    academic_year = table.Column<string>(type: "text", nullable: false),
                    surname = table.Column<string>(type: "text", nullable: true),
                    forename = table.Column<string>(type: "text", nullable: true),
                    sex = table.Column<string>(type: "text", nullable: true),
                    dob = table.Column<DateOnly>(type: "date", nullable: true),
                    data = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_further_education_learner_records", x => new { x.uln, x.dataset, x.academic_year });
                    table.ForeignKey(
                        name: "fk_further_education_learner_records_further_education_learner",
                        column: x => x.uln,
                        principalTable: "further_education_learners",
                        principalColumn: "uln",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "npd_pupil_records",
                columns: table => new
                {
                    pupil_matching_ref = table.Column<string>(type: "text", nullable: false),
                    dataset = table.Column<string>(type: "text", nullable: false),
                    academic_year = table.Column<string>(type: "text", nullable: false),
                    ingest_version = table.Column<short>(type: "smallint", nullable: false),
                    upn = table.Column<string>(type: "text", nullable: true),
                    la_code = table.Column<int>(type: "integer", nullable: true),
                    establishment_number = table.Column<int>(type: "integer", nullable: true),
                    urn = table.Column<int>(type: "integer", nullable: true),
                    surname = table.Column<string>(type: "text", nullable: true),
                    forename = table.Column<string>(type: "text", nullable: true),
                    middle_names = table.Column<string>(type: "text", nullable: true),
                    sex = table.Column<string>(type: "text", nullable: true),
                    dob = table.Column<DateOnly>(type: "date", nullable: true),
                    data = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_npd_pupil_records", x => new { x.pupil_matching_ref, x.dataset, x.academic_year });
                    table.CheckConstraint("ck_npd_pupil_records_ingest_version", "ingest_version BETWEEN 0 AND 2");
                    table.ForeignKey(
                        name: "fk_npd_pupil_records_npd_pupils_pupil_matching_ref",
                        column: x => x.pupil_matching_ref,
                        principalTable: "npd_pupils",
                        principalColumn: "pupil_matching_ref",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_further_education_learners_dob",
                table: "further_education_learners",
                column: "dob");

            migrationBuilder.CreateIndex(
                name: "ix_further_education_learners_forename",
                table: "further_education_learners",
                column: "forename")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_further_education_learners_surname",
                table: "further_education_learners",
                column: "surname")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_npd_pupil_records_dataset_academic_year",
                table: "npd_pupil_records",
                columns: new[] { "dataset", "academic_year" });

            migrationBuilder.CreateIndex(
                name: "ix_npd_pupils_dob",
                table: "npd_pupils",
                column: "dob");

            migrationBuilder.CreateIndex(
                name: "ix_npd_pupils_forename",
                table: "npd_pupils",
                column: "forename")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_npd_pupils_middle_names",
                table: "npd_pupils",
                column: "middle_names")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_npd_pupils_surname",
                table: "npd_pupils",
                column: "surname")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_npd_pupils_upn",
                table: "npd_pupils",
                column: "upn");

            migrationBuilder.CreateIndex(
                name: "ix_pupil_premium_pupils_dob",
                table: "pupil_premium_pupils",
                column: "dob");

            migrationBuilder.CreateIndex(
                name: "ix_pupil_premium_pupils_forename",
                table: "pupil_premium_pupils",
                column: "forename")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_pupil_premium_pupils_middle_names",
                table: "pupil_premium_pupils",
                column: "middle_names")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_pupil_premium_pupils_surname",
                table: "pupil_premium_pupils",
                column: "surname")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_pupil_premium_pupils_upn",
                table: "pupil_premium_pupils",
                column: "upn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "further_education_learner_records");

            migrationBuilder.DropTable(
                name: "npd_pupil_records");

            migrationBuilder.DropTable(
                name: "pupil_premium_pupils");

            migrationBuilder.DropTable(
                name: "further_education_learners");

            migrationBuilder.DropTable(
                name: "npd_pupils");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");
        }
    }
}
