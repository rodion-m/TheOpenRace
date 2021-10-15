using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenRace.Migrations
{
    public partial class AddMemberRegisteredBy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegisteredBy",
                table: "Members2021",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegisteredBy",
                table: "Members2021");
        }
    }
}
