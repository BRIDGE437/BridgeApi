using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace BridgeApi.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVectorEmbeddings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:vector", ",,");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "StartupProfiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Vector>(
                name: "Embedding",
                table: "StartupProfiles",
                type: "vector(384)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmbeddingHash",
                table: "StartupProfiles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HQ",
                table: "StartupProfiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "InvestorProfiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Vector>(
                name: "Embedding",
                table: "InvestorProfiles",
                type: "vector(384)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmbeddingHash",
                table: "InvestorProfiles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "StartupProfiles");

            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "StartupProfiles");

            migrationBuilder.DropColumn(
                name: "EmbeddingHash",
                table: "StartupProfiles");

            migrationBuilder.DropColumn(
                name: "HQ",
                table: "StartupProfiles");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "InvestorProfiles");

            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "InvestorProfiles");

            migrationBuilder.DropColumn(
                name: "EmbeddingHash",
                table: "InvestorProfiles");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}
