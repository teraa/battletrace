using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BattleTrace.Api.Migrations
{
    public partial class Add_Nav_Props : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_players_server_id",
                table: "players",
                column: "server_id");

            migrationBuilder.AddForeignKey(
                name: "fk_players_servers_server_id",
                table: "players",
                column: "server_id",
                principalTable: "servers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_players_servers_server_id",
                table: "players");

            migrationBuilder.DropIndex(
                name: "ix_players_server_id",
                table: "players");
        }
    }
}
