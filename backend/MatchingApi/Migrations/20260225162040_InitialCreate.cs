using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MatchingApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Investors",
                columns: table => new
                {
                    InvestorId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PreferredSectors = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PreferredBusinessModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PreferredRegions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PreferredCities = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InvestmentStage = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TicketSizeMin = table.Column<long>(type: "bigint", nullable: false),
                    TicketSizeMax = table.Column<long>(type: "bigint", nullable: false),
                    PreferredRevenueState = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Portfolio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LinkedIn = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Investors", x => x.InvestorId);
                });

            migrationBuilder.CreateTable(
                name: "Startups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Website = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Twitter = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Instagram = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    YearFounded = table.Column<int>(type: "integer", nullable: true),
                    HQ = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Founders = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Tags = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BusinessModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RevenueModel = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RevenueState = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TotalFunding = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Stage = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    WebsiteEmail = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WebsiteDescription = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Startups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InvestorId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartupId = table.Column<int>(type: "integer", nullable: false),
                    MatchingMode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TotalScore = table.Column<double>(type: "double precision", nullable: false),
                    SectorScore = table.Column<double>(type: "double precision", nullable: false),
                    GeoScore = table.Column<double>(type: "double precision", nullable: false),
                    ModelScore = table.Column<double>(type: "double precision", nullable: false),
                    StageScore = table.Column<double>(type: "double precision", nullable: false),
                    FundingBonus = table.Column<double>(type: "double precision", nullable: false),
                    SemanticScore = table.Column<double>(type: "double precision", nullable: false),
                    LlmBonus = table.Column<double>(type: "double precision", nullable: false),
                    AiReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchResults_Investors_InvestorId",
                        column: x => x.InvestorId,
                        principalTable: "Investors",
                        principalColumn: "InvestorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchResults_Startups_StartupId",
                        column: x => x.StartupId,
                        principalTable: "Startups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Investors_Active",
                table: "Investors",
                column: "Active");

            migrationBuilder.CreateIndex(
                name: "IX_Investors_Type",
                table: "Investors",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResults_CreatedAt",
                table: "MatchResults",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MatchResults_InvestorId_StartupId",
                table: "MatchResults",
                columns: new[] { "InvestorId", "StartupId" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchResults_StartupId",
                table: "MatchResults",
                column: "StartupId");

            migrationBuilder.CreateIndex(
                name: "IX_Startups_HQ",
                table: "Startups",
                column: "HQ");

            migrationBuilder.CreateIndex(
                name: "IX_Startups_Status",
                table: "Startups",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Startups_Tags",
                table: "Startups",
                column: "Tags");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchResults");

            migrationBuilder.DropTable(
                name: "Investors");

            migrationBuilder.DropTable(
                name: "Startups");
        }
    }
}
