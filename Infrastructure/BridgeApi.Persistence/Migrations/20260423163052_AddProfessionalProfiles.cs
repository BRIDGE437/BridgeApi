using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BridgeApi.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessionalProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvestorProfiles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PreferredSectors = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PreferredBusinessModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PreferredRegions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    InvestmentStage = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TicketSizeMin = table.Column<long>(type: "bigint", nullable: false),
                    TicketSizeMax = table.Column<long>(type: "bigint", nullable: false),
                    PreferredRevenueState = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Portfolio = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestorProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_InvestorProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StartupProfiles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Stage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tags = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    BusinessModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RevenueModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RevenueState = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TotalFunding = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WebsiteDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StartupProfiles", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_StartupProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestorProfiles");

            migrationBuilder.DropTable(
                name: "StartupProfiles");
        }
    }
}
