using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace OpenRace.Migrations
{
    public partial class AddPaymentNotificationSentAtField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Instant>(
                name: "PaymentNotificationSentAt",
                table: "Members2022_5",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentNotificationSentAt",
                table: "Members2022_5");
        }
    }
}
