using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymWeb.Migrations
{
    public partial class AddRoomBooking : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoomBookings",
                columns: table => new
                {
                    RoomBookingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MemberID = table.Column<int>(type: "int", nullable: false),
                    RoomID = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedByStaffID = table.Column<int>(type: "int", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomBookings", x => x.RoomBookingID);
                    table.ForeignKey(
                        name: "FK_RoomBookings_Members_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "MemberID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomBookings_Rooms_RoomID",
                        column: x => x.RoomID,
                        principalTable: "Rooms",
                        principalColumn: "RoomID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomBookings_Staffs_ProcessedByStaffID",
                        column: x => x.ProcessedByStaffID,
                        principalTable: "Staffs",
                        principalColumn: "StaffID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomBookings_MemberID",
                table: "RoomBookings",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_RoomBookings_ProcessedByStaffID",
                table: "RoomBookings",
                column: "ProcessedByStaffID");

            migrationBuilder.CreateIndex(
                name: "IX_RoomBookings_RoomID",
                table: "RoomBookings",
                column: "RoomID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomBookings");
        }
    }
}
