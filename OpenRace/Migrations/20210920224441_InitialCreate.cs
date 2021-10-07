using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace OpenRace.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Hash = table.Column<string>(type: "text", nullable: false),
                    PaidAt = table.Column<Instant>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members2021",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<Instant>(type: "timestamp", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    Distance = table.Column<double>(type: "double precision", nullable: false),
                    Referer = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<int>(type: "integer", nullable: true),
                    RaceResult = table.Column<Duration>(type: "interval", nullable: true),
                    PaymentId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members2021", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Members2021_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Members2021_Email",
                table: "Members2021",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Members2021_FullName",
                table: "Members2021",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_Members2021_PaymentId",
                table: "Members2021",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Members2021_Phone",
                table: "Members2021",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Hash",
                table: "Payments",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Id",
                table: "Payments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaidAt",
                table: "Payments",
                column: "PaidAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Members2021");

            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
