using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BridgeApi.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleSpecificProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WebsiteUrl",
                table: "UserProfiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "UserProfiles",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Surname",
                table: "UserProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProfileImage",
                table: "UserProfiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "UserProfiles",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UserProfiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "UserProfiles",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LinkedInUrl",
                table: "UserProfiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GitHubUrl",
                table: "UserProfiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "UserProfiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImage",
                table: "UserProfiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Headline",
                table: "UserProfiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnboardingCompletedAt",
                table: "UserProfiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FounderProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartupName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    StartupWebsite = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Stage = table.Column<int>(type: "integer", nullable: false),
                    PrimarySector = table.Column<int>(type: "integer", nullable: false),
                    SecondarySectors = table.Column<int[]>(type: "integer[]", nullable: false),
                    FundingNeedUsd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TeamSize = table.Column<int>(type: "integer", nullable: true),
                    PitchDeckUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OneLiner = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    ProblemStatement = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FoundedYear = table.Column<int>(type: "integer", nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActivelyFundraising = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FounderProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FounderProfiles_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestorProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirmName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    FirmWebsite = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CheckSizeMinUsd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CheckSizeMaxUsd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PreferredStages = table.Column<int[]>(type: "integer[]", nullable: false),
                    PreferredSectors = table.Column<int[]>(type: "integer[]", nullable: false),
                    PreferredGeographies = table.Column<string[]>(type: "text[]", nullable: false),
                    PortfolioCompanyCount = table.Column<int>(type: "integer", nullable: true),
                    InvestmentThesis = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsAcceptingPitches = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestorProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestorProfiles_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TalentProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Headline = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Skills = table.Column<string[]>(type: "text[]", nullable: false),
                    LookingFor = table.Column<int[]>(type: "integer[]", nullable: false),
                    WorkPreference = table.Column<int>(type: "integer", nullable: false),
                    YearsOfExperience = table.Column<int>(type: "integer", nullable: false),
                    ExpectedSalaryMonthlyUsd = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    OpenToWork = table.Column<bool>(type: "boolean", nullable: false),
                    AvailableFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CurrentRole = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CurrentCompany = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    InterestedSectors = table.Column<int[]>(type: "integer[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalentProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalentProfiles_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FounderProfiles_IsActivelyFundraising",
                table: "FounderProfiles",
                column: "IsActivelyFundraising");

            migrationBuilder.CreateIndex(
                name: "IX_FounderProfiles_PrimarySector",
                table: "FounderProfiles",
                column: "PrimarySector");

            migrationBuilder.CreateIndex(
                name: "IX_FounderProfiles_Stage",
                table: "FounderProfiles",
                column: "Stage");

            migrationBuilder.CreateIndex(
                name: "IX_FounderProfiles_UserProfileId",
                table: "FounderProfiles",
                column: "UserProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvestorProfiles_IsAcceptingPitches",
                table: "InvestorProfiles",
                column: "IsAcceptingPitches");

            migrationBuilder.CreateIndex(
                name: "IX_InvestorProfiles_UserProfileId",
                table: "InvestorProfiles",
                column: "UserProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TalentProfiles_OpenToWork",
                table: "TalentProfiles",
                column: "OpenToWork");

            migrationBuilder.CreateIndex(
                name: "IX_TalentProfiles_UserProfileId",
                table: "TalentProfiles",
                column: "UserProfileId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FounderProfiles");

            migrationBuilder.DropTable(
                name: "InvestorProfiles");

            migrationBuilder.DropTable(
                name: "TalentProfiles");

            migrationBuilder.DropColumn(
                name: "CoverImage",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "Headline",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "OnboardingCompletedAt",
                table: "UserProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "WebsiteUrl",
                table: "UserProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "UserProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Surname",
                table: "UserProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProfileImage",
                table: "UserProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "UserProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "UserProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "UserProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LinkedInUrl",
                table: "UserProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GitHubUrl",
                table: "UserProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "UserProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
