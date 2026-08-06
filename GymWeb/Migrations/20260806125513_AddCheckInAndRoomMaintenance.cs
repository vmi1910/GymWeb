using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymWeb.Migrations
{
    public partial class AddCheckInAndRoomMaintenance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckInHistories",
                columns: table => new
                {
                    CheckInID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberID = table.Column<int>(type: "int", nullable: false),
                    RoomID = table.Column<int>(type: "int", nullable: false),
                    CheckInTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckInHistories", x => x.CheckInID);
                    table.ForeignKey(
                        name: "FK_CheckInHistories_Members_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CheckInHistories_Rooms_RoomID",
                        column: x => x.RoomID,
                        principalTable: "Rooms",
                        principalColumn: "RoomID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoomMaintenances",
                columns: table => new
                {
                    RoomMaintenanceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomID = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByStaffID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomMaintenances", x => x.RoomMaintenanceID);
                    table.ForeignKey(
                        name: "FK_RoomMaintenances_Rooms_RoomID",
                        column: x => x.RoomID,
                        principalTable: "Rooms",
                        principalColumn: "RoomID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoomMaintenances_Staffs_CreatedByStaffID",
                        column: x => x.CreatedByStaffID,
                        principalTable: "Staffs",
                        principalColumn: "StaffID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckInHistories_MemberID",
                table: "CheckInHistories",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInHistories_RoomID_CheckOutTime",
                table: "CheckInHistories",
                columns: new[] { "RoomID", "CheckOutTime" });

            migrationBuilder.CreateIndex(
                name: "IX_RoomMaintenances_CreatedByStaffID",
                table: "RoomMaintenances",
                column: "CreatedByStaffID");

            migrationBuilder.CreateIndex(
                name: "IX_RoomMaintenances_RoomID_StartTime_EndTime",
                table: "RoomMaintenances",
                columns: new[] { "RoomID", "StartTime", "EndTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckInHistories");

            migrationBuilder.DropTable(
                name: "RoomMaintenances");
        }
    }
}
