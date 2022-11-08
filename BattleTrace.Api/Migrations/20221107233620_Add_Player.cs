using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BattleTrace.Api.Migrations
{
    public partial class Add_Player : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    server_id = table.Column<string>(type: "TEXT", nullable: false),
                    faction = table.Column<int>(type: "INTEGER", nullable: false),
                    team = table.Column<string>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    tag = table.Column<string>(type: "TEXT", nullable: false),
                    rank = table.Column<int>(type: "INTEGER", nullable: false),
                    score = table.Column<long>(type: "INTEGER", nullable: false),
                    kills = table.Column<int>(type: "INTEGER", nullable: false),
                    deaths = table.Column<int>(type: "INTEGER", nullable: false),
                    squad = table.Column<int>(type: "INTEGER", nullable: false),
                    role = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_players", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "players");
        }
    }
}
