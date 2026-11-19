using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DfE.GIAP.Core.Common.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class GiasReferenceData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "establishment_groups",
                columns: table => new
                {
                    group_uid = table.Column<int>(type: "integer", nullable: false),
                    group_id = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    group_type_code = table.Column<int>(type: "integer", nullable: false),
                    group_type_name = table.Column<string>(type: "text", nullable: false),
                    group_status_code = table.Column<string>(type: "text", nullable: false),
                    group_status_name = table.Column<string>(type: "text", nullable: false),
                    closed_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_establishment_groups", x => x.group_uid);
                });

            migrationBuilder.CreateTable(
                name: "local_authorities",
                columns: table => new
                {
                    la_code = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_local_authorities", x => x.la_code);
                });

            migrationBuilder.CreateTable(
                name: "establishment_group_links",
                columns: table => new
                {
                    group_uid = table.Column<int>(type: "integer", nullable: false),
                    urn = table.Column<int>(type: "integer", nullable: false),
                    joined_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_establishment_group_links", x => new { x.group_uid, x.urn });
                    table.ForeignKey(
                        name: "fk_establishment_group_links_establishment_groups_group_uid",
                        column: x => x.group_uid,
                        principalTable: "establishment_groups",
                        principalColumn: "group_uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "establishments",
                columns: table => new
                {
                    urn = table.Column<int>(type: "integer", nullable: false),
                    la_code = table.Column<int>(type: "integer", nullable: false),
                    establishment_number = table.Column<int>(type: "integer", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    uprn = table.Column<long>(type: "bigint", nullable: true),
                    type_of_establishment_code = table.Column<int>(type: "integer", nullable: false),
                    type_of_establishment_name = table.Column<string>(type: "text", nullable: false),
                    establishment_status_code = table.Column<int>(type: "integer", nullable: false),
                    establishment_status_name = table.Column<string>(type: "text", nullable: false),
                    close_date = table.Column<DateOnly>(type: "date", nullable: true),
                    phase_of_education_code = table.Column<int>(type: "integer", nullable: false),
                    phase_of_education_name = table.Column<string>(type: "text", nullable: false),
                    statutory_low_age = table.Column<int>(type: "integer", nullable: true),
                    statutory_high_age = table.Column<int>(type: "integer", nullable: true),
                    gor_code = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_establishments", x => x.urn);
                    table.ForeignKey(
                        name: "fk_establishments_local_authorities_la_code",
                        column: x => x.la_code,
                        principalTable: "local_authorities",
                        principalColumn: "la_code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_establishment_group_links_urn",
                table: "establishment_group_links",
                column: "urn");

            migrationBuilder.CreateIndex(
                name: "ix_establishments_la_code_establishment_number",
                table: "establishments",
                columns: new[] { "la_code", "establishment_number" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "establishment_group_links");

            migrationBuilder.DropTable(
                name: "establishments");

            migrationBuilder.DropTable(
                name: "establishment_groups");

            migrationBuilder.DropTable(
                name: "local_authorities");
        }
    }
}
