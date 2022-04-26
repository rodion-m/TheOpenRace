using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace OpenRace.Migrations
{
    public partial class AddTableMembers2022_5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members2021_Payments_PaymentId",
                table: "Members2021");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Members2021",
                table: "Members2021");

            migrationBuilder.RenameTable(
                name: "Members2021",
                newName: "Members2022_5");

            migrationBuilder.RenameIndex(
                name: "IX_Members2021_Phone",
                table: "Members2022_5",
                newName: "IX_Members2022_5_Phone");

            migrationBuilder.RenameIndex(
                name: "IX_Members2021_PaymentId",
                table: "Members2022_5",
                newName: "IX_Members2022_5_PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_Members2021_FullName",
                table: "Members2022_5",
                newName: "IX_Members2022_5_FullName");

            migrationBuilder.RenameIndex(
                name: "IX_Members2021_Email",
                table: "Members2022_5",
                newName: "IX_Members2022_5_Email");

            migrationBuilder.AlterColumn<Instant>(
                name: "Result",
                table: "Results",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "PaidAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "TimeStamp",
                table: "Events",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "CreatedAt",
                table: "Members2022_5",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<string>(
                name: "ParentId",
                table: "Members2022_5",
                type: "text",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Members2022_5",
                table: "Members2022_5",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Members2022_5_Payments_PaymentId",
                table: "Members2022_5",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members2022_5_Payments_PaymentId",
                table: "Members2022_5");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Members2022_5",
                table: "Members2022_5");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Members2022_5");

            migrationBuilder.RenameTable(
                name: "Members2022_5",
                newName: "Members2021");

            migrationBuilder.RenameIndex(
                name: "IX_Members2022_5_Phone",
                table: "Members2021",
                newName: "IX_Members2021_Phone");

            migrationBuilder.RenameIndex(
                name: "IX_Members2022_5_PaymentId",
                table: "Members2021",
                newName: "IX_Members2021_PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_Members2022_5_FullName",
                table: "Members2021",
                newName: "IX_Members2021_FullName");

            migrationBuilder.RenameIndex(
                name: "IX_Members2022_5_Email",
                table: "Members2021",
                newName: "IX_Members2021_Email");

            migrationBuilder.AlterColumn<Instant>(
                name: "Result",
                table: "Results",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "PaidAt",
                table: "Payments",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "TimeStamp",
                table: "Events",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "CreatedAt",
                table: "Members2021",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Members2021",
                table: "Members2021",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Members2021_Payments_PaymentId",
                table: "Members2021",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");
        }
    }
}
