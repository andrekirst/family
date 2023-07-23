using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class labeling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "weighttracking");

            migrationBuilder.CreateTable(
                name: "Labels",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Color = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(265)", maxLength: 265, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    RowVersion = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Labels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeightTrackingEntries",
                schema: "weighttracking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeasuredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeightUnit = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    FamilyMemberId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    RowVersion = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeightTrackingEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeightTrackingEntries_FamilyMembers_FamilyMemberId",
                        column: x => x.FamilyMemberId,
                        principalSchema: "core",
                        principalTable: "FamilyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FamilyMemberLabel",
                schema: "core",
                columns: table => new
                {
                    FamilyMembersId = table.Column<int>(type: "int", nullable: false),
                    LabelsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyMemberLabel", x => new { x.FamilyMembersId, x.LabelsId });
                    table.ForeignKey(
                        name: "FK_FamilyMemberLabel_FamilyMembers_FamilyMembersId",
                        column: x => x.FamilyMembersId,
                        principalSchema: "core",
                        principalTable: "FamilyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FamilyMemberLabel_Labels_LabelsId",
                        column: x => x.LabelsId,
                        principalSchema: "core",
                        principalTable: "Labels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMemberLabel_LabelsId",
                schema: "core",
                table: "FamilyMemberLabel",
                column: "LabelsId");

            migrationBuilder.CreateIndex(
                name: "IX_WeightTrackingEntries_FamilyMemberId",
                schema: "weighttracking",
                table: "WeightTrackingEntries",
                column: "FamilyMemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FamilyMemberLabel",
                schema: "core");

            migrationBuilder.DropTable(
                name: "WeightTrackingEntries",
                schema: "weighttracking");

            migrationBuilder.DropTable(
                name: "Labels",
                schema: "core");
        }
    }
}
