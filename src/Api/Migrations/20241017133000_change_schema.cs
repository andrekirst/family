using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class change_schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "authentiction");

            migrationBuilder.RenameTable(
                name: "GoogleAccounts",
                schema: "core",
                newName: "GoogleAccounts",
                newSchema: "authentiction");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "GoogleAccounts",
                schema: "authentiction",
                newName: "GoogleAccounts",
                newSchema: "core");
        }
    }
}
