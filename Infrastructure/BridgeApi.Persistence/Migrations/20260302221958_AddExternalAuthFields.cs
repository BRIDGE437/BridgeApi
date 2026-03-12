using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BridgeApi.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalAuthFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthProvider",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProviderKey",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthProvider",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProviderKey",
                table: "AspNetUsers");
        }
    }
}
