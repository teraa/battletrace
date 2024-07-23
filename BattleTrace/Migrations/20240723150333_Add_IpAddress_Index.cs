using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BattleTrace.Migrations
{
    /// <inheritdoc />
    public partial class Add_IpAddress_Index : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_servers_ip_address",
                table: "servers",
                column: "ip_address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_servers_ip_address",
                table: "servers");
        }
    }
}
