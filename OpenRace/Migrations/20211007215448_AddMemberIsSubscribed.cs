using Microsoft.EntityFrameworkCore.Migrations;

namespace OpenRace.Migrations
{
    public partial class AddMemberIsSubscribed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Distance",
                table: "Members2021");

            migrationBuilder.AddColumn<int>(
                name: "DistanceMt",
                table: "Members2021",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistanceMt",
                table: "Members2021");

            migrationBuilder.AddColumn<double>(
                name: "Distance",
                table: "Members2021",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
