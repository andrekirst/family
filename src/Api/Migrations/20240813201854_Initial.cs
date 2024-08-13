using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.EnsureSchema(
                name: "weighttracking");

            migrationBuilder.CreateTable(
                name: "FamilyMembers",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LastName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Birthdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AspNetUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: true),
                    RowVersion = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Labels",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Color = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(265)", maxLength: 265, nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: true),
                    RowVersion = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Labels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DomainEventEntries",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventType = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: false),
                    EventVersion = table.Column<int>(type: "integer", nullable: false),
                    EventData = table.Column<string>(type: "text", nullable: false),
                    CreatedByFamilyMemberId = table.Column<int>(type: "integer", nullable: true),
                    CreatedForFamilyMemberId = table.Column<int>(type: "integer", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: true),
                    RowVersion = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainEventEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DomainEventEntries_FamilyMembers_CreatedByFamilyMemberId",
                        column: x => x.CreatedByFamilyMemberId,
                        principalSchema: "core",
                        principalTable: "FamilyMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DomainEventEntries_FamilyMembers_CreatedForFamilyMemberId",
                        column: x => x.CreatedForFamilyMemberId,
                        principalSchema: "core",
                        principalTable: "FamilyMembers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WeightTrackingEntries",
                schema: "weighttracking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MeasuredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WeightUnit = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: false),
                    Weight = table.Column<double>(type: "double precision", nullable: false),
                    FamilyMemberId = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: true),
                    RowVersion = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, rowVersion: true, nullable: true)
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
                    FamilyMembersId = table.Column<int>(type: "integer", nullable: false),
                    LabelsId = table.Column<int>(type: "integer", nullable: false)
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
                name: "IX_DomainEventEntries_CreatedByFamilyMemberId",
                schema: "core",
                table: "DomainEventEntries",
                column: "CreatedByFamilyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_DomainEventEntries_CreatedForFamilyMemberId",
                schema: "core",
                table: "DomainEventEntries",
                column: "CreatedForFamilyMemberId");

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
                name: "DomainEventEntries",
                schema: "core");

            migrationBuilder.DropTable(
                name: "FamilyMemberLabel",
                schema: "core");

            migrationBuilder.DropTable(
                name: "WeightTrackingEntries",
                schema: "weighttracking");

            migrationBuilder.DropTable(
                name: "Labels",
                schema: "core");

            migrationBuilder.DropTable(
                name: "FamilyMembers",
                schema: "core");
        }
    }
}
