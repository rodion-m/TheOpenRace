using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenRace.Migrations
{
    public partial class RenameDistanceMtToDistance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DistanceMt",
                table: "Members2021",
                newName: "Distance");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Distance",
                table: "Members2021",
                newName: "DistanceMt");
        }
    }
}
