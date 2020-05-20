using Microsoft.EntityFrameworkCore.Migrations;

namespace api.Migrations
{
    public partial class AddIsDeviceOnline : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeviceOnline",
                table: "DeviceTokens",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeviceOnline",
                table: "DeviceTokens");
        }
    }
}
