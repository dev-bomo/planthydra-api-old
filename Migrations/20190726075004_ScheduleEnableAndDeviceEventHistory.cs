using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace api.Migrations
{
    public partial class ScheduleEnableAndDeviceEventHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isEnabled",
                table: "WateringSchedules",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DeviceEvent",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DeviceId = table.Column<int>(nullable: true),
                    EventDate = table.Column<DateTime>(nullable: false),
                    IsOnline = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceEvent_DeviceTokens_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "DeviceTokens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvent_DeviceId",
                table: "DeviceEvent",
                column: "DeviceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceEvent");

            migrationBuilder.DropColumn(
                name: "isEnabled",
                table: "WateringSchedules");
        }
    }
}
