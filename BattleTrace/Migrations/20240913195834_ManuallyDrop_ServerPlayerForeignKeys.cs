using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BattleTrace.Migrations
{
    /// <inheritdoc />
    public partial class ManuallyDrop_ServerPlayerForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_players_servers_server_id",
                table: "players");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "fk_players_servers_server_id",
                table: "players",
                column: "server_id",
                principalTable: "servers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
