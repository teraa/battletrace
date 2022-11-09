using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BattleTrace.Api.Migrations
{
    public partial class Add_DTO_Migration3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updated_at2",
                table: "servers",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "updated_at2",
                table: "players",
                newName: "updated_at");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "servers",
                newName: "updated_at2");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "players",
                newName: "updated_at2");
        }
    }
}
