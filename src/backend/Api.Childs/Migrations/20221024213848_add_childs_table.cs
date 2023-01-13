using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Childs.Migrations
{
    public partial class add_childs_table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Childs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    LastName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CountryAlpha3Code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    CityOfBirth = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Childs", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Childs");
        }
    }
}
