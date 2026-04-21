using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MatchingApi.Migrations
{
    /// <inheritdoc />
    public partial class AddEventMatchingModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "MatchResults",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MatchEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TopMatchingCount = table.Column<int>(type: "integer", nullable: false),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FilterValue = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventParticipations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventId = table.Column<int>(type: "integer", nullable: false),
                    ParticipantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParticipantType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventParticipations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventParticipations_MatchEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "MatchEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchResults_EventId",
                table: "MatchResults",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipations_EventId_ParticipantId_ParticipantType",
                table: "EventParticipations",
                columns: new[] { "EventId", "ParticipantId", "ParticipantType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvents_ScheduledAt",
                table: "MatchEvents",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvents_Status",
                table: "MatchEvents",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchResults_MatchEvents_EventId",
                table: "MatchResults",
                column: "EventId",
                principalTable: "MatchEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchResults_MatchEvents_EventId",
                table: "MatchResults");

            migrationBuilder.DropTable(
                name: "EventParticipations");

            migrationBuilder.DropTable(
                name: "MatchEvents");

            migrationBuilder.DropIndex(
                name: "IX_MatchResults_EventId",
                table: "MatchResults");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "MatchResults");
        }
    }
}
