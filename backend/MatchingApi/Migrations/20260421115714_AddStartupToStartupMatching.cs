using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MatchingApi.Migrations
{
    /// <inheritdoc />
    public partial class AddStartupToStartupMatching : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StartupMatchResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: true),
                    SourceStartupId = table.Column<int>(type: "integer", nullable: false),
                    TargetStartupId = table.Column<int>(type: "integer", nullable: false),
                    TotalScore = table.Column<double>(type: "double precision", nullable: false),
                    SectorScore = table.Column<double>(type: "double precision", nullable: false),
                    GeoScore = table.Column<double>(type: "double precision", nullable: false),
                    StageScore = table.Column<double>(type: "double precision", nullable: false),
                    SemanticScore = table.Column<double>(type: "double precision", nullable: false),
                    LlmBonus = table.Column<double>(type: "double precision", nullable: false),
                    AiReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StartupMatchResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StartupMatchResults_MatchEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "MatchEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StartupMatchResults_Startups_SourceStartupId",
                        column: x => x.SourceStartupId,
                        principalTable: "Startups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StartupMatchResults_Startups_TargetStartupId",
                        column: x => x.TargetStartupId,
                        principalTable: "Startups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StartupMatchResults_CreatedAt",
                table: "StartupMatchResults",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_StartupMatchResults_EventId",
                table: "StartupMatchResults",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_StartupMatchResults_SourceStartupId_TargetStartupId",
                table: "StartupMatchResults",
                columns: new[] { "SourceStartupId", "TargetStartupId" });

            migrationBuilder.CreateIndex(
                name: "IX_StartupMatchResults_TargetStartupId",
                table: "StartupMatchResults",
                column: "TargetStartupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StartupMatchResults");
        }
    }
}
