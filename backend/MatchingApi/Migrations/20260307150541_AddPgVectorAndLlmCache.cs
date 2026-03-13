using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pgvector;

#nullable disable

namespace MatchingApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPgVectorAndLlmCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.AddColumn<Vector>(
                name: "Embedding",
                table: "Startups",
                type: "vector(384)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmbeddingHash",
                table: "Startups",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<Vector>(
                name: "Embedding",
                table: "Investors",
                type: "vector(384)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmbeddingHash",
                table: "Investors",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LlmScoreCache",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvestorTextHash = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    StartupId = table.Column<int>(type: "integer", nullable: false),
                    StartupTextHash = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Score = table.Column<double>(type: "double precision", nullable: false),
                    Reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LlmScoreCache", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LlmScoreCache_CreatedAt",
                table: "LlmScoreCache",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LlmScoreCache_InvestorTextHash_StartupId_StartupTextHash",
                table: "LlmScoreCache",
                columns: new[] { "InvestorTextHash", "StartupId", "StartupTextHash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LlmScoreCache");

            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "EmbeddingHash",
                table: "Startups");

            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "Investors");

            migrationBuilder.DropColumn(
                name: "EmbeddingHash",
                table: "Investors");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}
