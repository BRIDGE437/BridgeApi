using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BridgeApi.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNeedsManualReviewFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NeedsManualReview",
                table: "StartupProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeedsManualReview",
                table: "StartupProfiles");
        }
    }
}
