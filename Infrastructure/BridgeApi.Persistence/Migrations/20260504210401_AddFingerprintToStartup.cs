using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BridgeApi.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFingerprintToStartup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EmbeddingHash",
                table: "StartupProfiles",
                type: "character varying(46)",
                maxLength: 46,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalFingerprint",
                table: "StartupProfiles",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StartupProfiles_ExternalFingerprint",
                table: "StartupProfiles",
                column: "ExternalFingerprint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StartupProfiles_ExternalFingerprint",
                table: "StartupProfiles");

            migrationBuilder.DropColumn(
                name: "ExternalFingerprint",
                table: "StartupProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "EmbeddingHash",
                table: "StartupProfiles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(46)",
                oldMaxLength: 46,
                oldNullable: true);
        }
    }
}
