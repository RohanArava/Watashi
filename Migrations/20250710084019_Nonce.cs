using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watashi.Migrations
{
    /// <inheritdoc />
    public partial class Nonce : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nonce",
                table: "AuthorizationCodes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nonce",
                table: "AuthorizationCodes");
        }
    }
}
