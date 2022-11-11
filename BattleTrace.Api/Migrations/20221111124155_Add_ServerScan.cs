using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BattleTrace.Api.Migrations
{
    public partial class Add_ServerScan : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "server_scans",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    timestamp = table.Column<long>(type: "INTEGER", nullable: false),
                    server_count = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_server_scans", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_server_scans_timestamp",
                table: "server_scans",
                column: "timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "server_scans");
        }
    }
}
