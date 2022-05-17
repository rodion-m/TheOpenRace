using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenRace.Migrations
{
    public partial class RemoveParentId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Members2022_5");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "Members2022_5",
                type: "uuid",
                nullable: true);
        }
    }
}
