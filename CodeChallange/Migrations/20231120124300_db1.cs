using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeChallange.Migrations
{
    public partial class db1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ManagerId",
                table: "users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_ManagerId",
                table: "users",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_users_users_ManagerId",
                table: "users",
                column: "ManagerId",
                principalTable: "users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_users_ManagerId",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_ManagerId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "users");
        }
    }
}
