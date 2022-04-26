using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenRace.Migrations
{
    public partial class TechUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentId",
                table: "Members2022_5",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Members2022_5");
        }
    }
}
