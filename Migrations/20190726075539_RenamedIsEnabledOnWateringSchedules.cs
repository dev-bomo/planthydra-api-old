using Microsoft.EntityFrameworkCore.Migrations;

namespace api.Migrations
{
    public partial class RenamedIsEnabledOnWateringSchedules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isEnabled",
                table: "WateringSchedules",
                newName: "IsEnabled");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsEnabled",
                table: "WateringSchedules",
                newName: "isEnabled");
        }
    }
}
