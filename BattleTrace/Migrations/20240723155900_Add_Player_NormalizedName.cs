using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BattleTrace.Migrations
{
    /// <inheritdoc />
    public partial class Add_Player_NormalizedName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_players_name",
                table: "players");

            migrationBuilder.AddColumn<string>(
                name: "normalized_name",
                table: "players",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("update players set normalized_name = lower(name);");

            migrationBuilder.CreateIndex(
                name: "ix_players_normalized_name",
                table: "players",
                column: "normalized_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_players_normalized_name",
                table: "players");

            migrationBuilder.DropColumn(
                name: "normalized_name",
                table: "players");

            migrationBuilder.CreateIndex(
                name: "ix_players_name",
                table: "players",
                column: "name");
        }
    }
}
