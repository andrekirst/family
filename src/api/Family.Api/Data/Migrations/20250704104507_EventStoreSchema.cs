using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Family.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class EventStoreSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "i_x__users__keycloak_subject_id",
                table: "users",
                newName: "ix_users_keycloak_subject_id");

            migrationBuilder.RenameIndex(
                name: "i_x__users__email",
                table: "users",
                newName: "ix_users_email");

            migrationBuilder.RenameIndex(
                name: "i_x__user_roles__user_id__role_name",
                table: "user_roles",
                newName: "ix_user_roles_user_id_role_name");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "user_roles",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.CreateTable(
                name: "events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    aggregate_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    aggregate_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    event_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    event_data = table.Column<string>(type: "jsonb", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    version = table.Column<int>(type: "integer", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    causation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    aggregate_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    aggregate_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_snapshots", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_events_aggregate_id",
                table: "events",
                column: "aggregate_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_aggregate_id_version",
                table: "events",
                columns: new[] { "aggregate_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_events_aggregate_type",
                table: "events",
                column: "aggregate_type");

            migrationBuilder.CreateIndex(
                name: "ix_events_correlation_id",
                table: "events",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_events_event_type",
                table: "events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "ix_events_timestamp",
                table: "events",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_events_user_id",
                table: "events",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_snapshots_aggregate_id",
                table: "snapshots",
                column: "aggregate_id");

            migrationBuilder.CreateIndex(
                name: "ix_snapshots_aggregate_id_version",
                table: "snapshots",
                columns: new[] { "aggregate_id", "version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_snapshots_aggregate_type",
                table: "snapshots",
                column: "aggregate_type");

            migrationBuilder.CreateIndex(
                name: "ix_snapshots_timestamp",
                table: "snapshots",
                column: "timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "events");

            migrationBuilder.DropTable(
                name: "snapshots");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "user_roles");

            migrationBuilder.RenameIndex(
                name: "ix_users_keycloak_subject_id",
                table: "users",
                newName: "i_x__users__keycloak_subject_id");

            migrationBuilder.RenameIndex(
                name: "ix_users_email",
                table: "users",
                newName: "i_x__users__email");

            migrationBuilder.RenameIndex(
                name: "ix_user_roles_user_id_role_name",
                table: "user_roles",
                newName: "i_x__user_roles__user_id__role_name");
        }
    }
}
