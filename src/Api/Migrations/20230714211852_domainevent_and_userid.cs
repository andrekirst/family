using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class domainevent_and_userid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AspNetUserId",
                schema: "core",
                table: "FamilyMembers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DomainEventEntries",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    EventVersion = table.Column<int>(type: "int", nullable: false),
                    EventData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    RowVersion = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainEventEntries", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DomainEventEntries",
                schema: "core");

            migrationBuilder.DropColumn(
                name: "AspNetUserId",
                schema: "core",
                table: "FamilyMembers");
        }
    }
}
