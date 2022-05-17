using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenRace.Migrations
{
    public partial class AddParentName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParentName",
                table: "Members2022_5",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentName",
                table: "Members2022_5");
        }
    }
}
