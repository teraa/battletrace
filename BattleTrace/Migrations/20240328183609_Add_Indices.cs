using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BattleTrace.Migrations
{
    /// <inheritdoc />
    public partial class Add_Indices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_servers_name",
                table: "servers",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_players_name",
                table: "players",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_players_tag",
                table: "players",
                column: "tag");

            migrationBuilder.CreateIndex(
                name: "ix_players_updated_at",
                table: "players",
                column: "updated_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_servers_name",
                table: "servers");

            migrationBuilder.DropIndex(
                name: "ix_players_name",
                table: "players");

            migrationBuilder.DropIndex(
                name: "ix_players_tag",
                table: "players");

            migrationBuilder.DropIndex(
                name: "ix_players_updated_at",
                table: "players");
        }
    }
}
