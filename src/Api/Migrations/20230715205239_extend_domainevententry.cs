using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class extend_domainevententry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByFamilyMemberId",
                schema: "core",
                table: "DomainEventEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedForFamilyMemberId",
                schema: "core",
                table: "DomainEventEntries",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DomainEventEntries_CreatedByFamilyMemberId",
                schema: "core",
                table: "DomainEventEntries",
                column: "CreatedByFamilyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_DomainEventEntries_CreatedForFamilyMemberId",
                schema: "core",
                table: "DomainEventEntries",
                column: "CreatedForFamilyMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_DomainEventEntries_FamilyMembers_CreatedByFamilyMemberId",
                schema: "core",
                table: "DomainEventEntries",
                column: "CreatedByFamilyMemberId",
                principalSchema: "core",
                principalTable: "FamilyMembers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DomainEventEntries_FamilyMembers_CreatedForFamilyMemberId",
                schema: "core",
                table: "DomainEventEntries",
                column: "CreatedForFamilyMemberId",
                principalSchema: "core",
                principalTable: "FamilyMembers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DomainEventEntries_FamilyMembers_CreatedByFamilyMemberId",
                schema: "core",
                table: "DomainEventEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_DomainEventEntries_FamilyMembers_CreatedForFamilyMemberId",
                schema: "core",
                table: "DomainEventEntries");

            migrationBuilder.DropIndex(
                name: "IX_DomainEventEntries_CreatedByFamilyMemberId",
                schema: "core",
                table: "DomainEventEntries");

            migrationBuilder.DropIndex(
                name: "IX_DomainEventEntries_CreatedForFamilyMemberId",
                schema: "core",
                table: "DomainEventEntries");

            migrationBuilder.DropColumn(
                name: "CreatedByFamilyMemberId",
                schema: "core",
                table: "DomainEventEntries");

            migrationBuilder.DropColumn(
                name: "CreatedForFamilyMemberId",
                schema: "core",
                table: "DomainEventEntries");
        }
    }
}
