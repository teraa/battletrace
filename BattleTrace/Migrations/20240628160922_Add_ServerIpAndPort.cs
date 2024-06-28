using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BattleTrace.Migrations
{
    /// <inheritdoc />
    public partial class Add_ServerIpAndPort : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ip_address",
                table: "servers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "port",
                table: "servers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ip_address",
                table: "servers");

            migrationBuilder.DropColumn(
                name: "port",
                table: "servers");
        }
    }
}
