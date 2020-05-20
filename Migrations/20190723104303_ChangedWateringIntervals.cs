using Microsoft.EntityFrameworkCore.Migrations;

namespace api.Migrations
{
    public partial class ChangedWateringIntervals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Interval",
                table: "WateringSchedules");

            migrationBuilder.AddColumn<string>(
                name: "WateringDays",
                table: "WateringSchedules",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WateringDays",
                table: "WateringSchedules");

            migrationBuilder.AddColumn<int>(
                name: "Interval",
                table: "WateringSchedules",
                nullable: false,
                defaultValue: 0);
        }
    }
}
