using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpenRace.Migrations
{
    public partial class RenameMembersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members2022_5_Payments_PaymentId",
                table: "Members2022_5");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Members2022_5",
                table: "Members2022_5");

            migrationBuilder.RenameTable(
                name: "Members2022_5",
                newName: "Members");

            migrationBuilder.RenameIndex(
                name: "IX_Members2022_5_Phone",
                table: "Members",
                newName: "IX_Members_Phone");

            migrationBuilder.RenameIndex(
                name: "IX_Members2022_5_PaymentId",
                table: "Members",
                newName: "IX_Members_PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_Members2022_5_FullName",
                table: "Members",
                newName: "IX_Members_FullName");

            migrationBuilder.RenameIndex(
                name: "IX_Members2022_5_Email",
                table: "Members",
                newName: "IX_Members_Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Members",
                table: "Members",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Payments_PaymentId",
                table: "Members",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Payments_PaymentId",
                table: "Members");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Members",
                table: "Members");

            migrationBuilder.RenameTable(
                name: "Members",
                newName: "Members2022_5");

            migrationBuilder.RenameIndex(
                name: "IX_Members_Phone",
                table: "Members2022_5",
                newName: "IX_Members2022_5_Phone");

            migrationBuilder.RenameIndex(
                name: "IX_Members_PaymentId",
                table: "Members2022_5",
                newName: "IX_Members2022_5_PaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_Members_FullName",
                table: "Members2022_5",
                newName: "IX_Members2022_5_FullName");

            migrationBuilder.RenameIndex(
                name: "IX_Members_Email",
                table: "Members2022_5",
                newName: "IX_Members2022_5_Email");

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
    }
}
