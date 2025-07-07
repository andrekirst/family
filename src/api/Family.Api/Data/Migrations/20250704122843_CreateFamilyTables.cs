using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Family.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreateFamilyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "families",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_families", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "family_members",
                columns: table => new
                {
                    family_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_family_members", x => new { x.family_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_family_members_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_families_created_at",
                table: "families",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_families_name",
                table: "families",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_families_owner_id",
                table: "families",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_members_family_id",
                table: "family_members",
                column: "family_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_members_joined_at",
                table: "family_members",
                column: "joined_at");

            migrationBuilder.CreateIndex(
                name: "ix_family_members_role",
                table: "family_members",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "uq_family_members_user_id",
                table: "family_members",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "family_members");

            migrationBuilder.DropTable(
                name: "families");
        }
    }
}
