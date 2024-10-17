using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class notification_messages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WeightTrackingEntries_FamilyMembers_FamilyMemberId",
                schema: "weighttracking",
                table: "WeightTrackingEntries");

            migrationBuilder.DropTable(
                name: "DomainEventEntries",
                schema: "core");

            migrationBuilder.DropTable(
                name: "FamilyMemberLabel",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "IX_WeightTrackingEntries_FamilyMemberId",
                schema: "weighttracking",
                table: "WeightTrackingEntries");

            migrationBuilder.EnsureSchema(
                name: "calendar");

            migrationBuilder.AddColumn<Guid>(
                name: "FamilyMemberEntityId",
                schema: "weighttracking",
                table: "WeightTrackingEntries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Calendars",
                schema: "calendar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FamilyMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: true),
                    RowVersion = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calendars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Calendars_FamilyMembers_FamilyMemberId",
                        column: x => x.FamilyMemberId,
                        principalSchema: "core",
                        principalTable: "FamilyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DomainEvents",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: false),
                    EventVersion = table.Column<int>(type: "integer", nullable: false),
                    EventData = table.Column<string>(type: "text", nullable: false),
                    CreatedByFamilyMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedForFamilyMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: true),
                    RowVersion = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DomainEvents_FamilyMembers_CreatedByFamilyMemberId",
                        column: x => x.CreatedByFamilyMemberId,
                        principalSchema: "core",
                        principalTable: "FamilyMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DomainEvents_FamilyMembers_CreatedForFamilyMemberId",
                        column: x => x.CreatedForFamilyMemberId,
                        principalSchema: "core",
                        principalTable: "FamilyMembers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FamilyMemberEntityLabelEntity",
                schema: "core",
                columns: table => new
                {
                    FamilyMembersId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabelsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyMemberEntityLabelEntity", x => new { x.FamilyMembersId, x.LabelsId });
                    table.ForeignKey(
                        name: "FK_FamilyMemberEntityLabelEntity_FamilyMembers_FamilyMembersId",
                        column: x => x.FamilyMembersId,
                        principalSchema: "core",
                        principalTable: "FamilyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FamilyMemberEntityLabelEntity_Labels_LabelsId",
                        column: x => x.LabelsId,
                        principalSchema: "core",
                        principalTable: "Labels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificatioMessages",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MarkedAsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FamilyMemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageType = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: false),
                    Version = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: false),
                    Instance = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: true),
                    RowVersion = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificatioMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificatioMessages_FamilyMembers_FamilyMemberId",
                        column: x => x.FamilyMemberId,
                        principalSchema: "core",
                        principalTable: "FamilyMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeightTrackingEntries_FamilyMemberEntityId",
                schema: "weighttracking",
                table: "WeightTrackingEntries",
                column: "FamilyMemberEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_FamilyMemberId",
                schema: "calendar",
                table: "Calendars",
                column: "FamilyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_DomainEvents_CreatedByFamilyMemberId",
                schema: "core",
                table: "DomainEvents",
                column: "CreatedByFamilyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_DomainEvents_CreatedForFamilyMemberId",
                schema: "core",
                table: "DomainEvents",
                column: "CreatedForFamilyMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMemberEntityLabelEntity_LabelsId",
                schema: "core",
                table: "FamilyMemberEntityLabelEntity",
                column: "LabelsId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificatioMessages_FamilyMemberId",
                schema: "core",
                table: "NotificatioMessages",
                column: "FamilyMemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_WeightTrackingEntries_FamilyMembers_FamilyMemberEntityId",
                schema: "weighttracking",
                table: "WeightTrackingEntries",
                column: "FamilyMemberEntityId",
                principalSchema: "core",
                principalTable: "FamilyMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WeightTrackingEntries_FamilyMembers_FamilyMemberEntityId",
                schema: "weighttracking",
                table: "WeightTrackingEntries");

            migrationBuilder.DropTable(
                name: "Calendars",
                schema: "calendar");

            migrationBuilder.DropTable(
                name: "DomainEvents",
                schema: "core");

            migrationBuilder.DropTable(
                name: "FamilyMemberEntityLabelEntity",
                schema: "core");

            migrationBuilder.DropTable(
                name: "NotificatioMessages",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "IX_WeightTrackingEntries_FamilyMemberEntityId",
                schema: "weighttracking",
                table: "WeightTrackingEntries");

            migrationBuilder.DropColumn(
                name: "FamilyMemberEntityId",
                schema: "weighttracking",
                table: "WeightTrackingEntries");

            migrationBuilder.CreateTable(
                name: "DomainEventEntries",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedByFamilyMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedForFamilyMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EventData = table.Column<string>(type: "text", nullable: false),
                    EventType = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: false),
                    EventVersion = table.Column<int>(type: "integer", nullable: false),
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
                name: "FamilyMemberLabel",
                schema: "core",
                columns: table => new
                {
                    FamilyMembersId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabelsId = table.Column<Guid>(type: "uuid", nullable: false)
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
                name: "IX_WeightTrackingEntries_FamilyMemberId",
                schema: "weighttracking",
                table: "WeightTrackingEntries",
                column: "FamilyMemberId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_WeightTrackingEntries_FamilyMembers_FamilyMemberId",
                schema: "weighttracking",
                table: "WeightTrackingEntries",
                column: "FamilyMemberId",
                principalSchema: "core",
                principalTable: "FamilyMembers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
