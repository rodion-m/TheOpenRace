using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace OpenRace.Migrations
{
    public partial class AddEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberNumber = table.Column<int>(type: "integer", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    TimeStamp = table.Column<Instant>(type: "timestamp", nullable: false),
                    CreatorName = table.Column<string>(type: "text", nullable: false),
                    Distance = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_Distance",
                table: "Events",
                column: "Distance");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventType",
                table: "Events",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_Events_MemberNumber",
                table: "Events",
                column: "MemberNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Events_RaceId",
                table: "Events",
                column: "RaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_TimeStamp",
                table: "Events",
                column: "TimeStamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
