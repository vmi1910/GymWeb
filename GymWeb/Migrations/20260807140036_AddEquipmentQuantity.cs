using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymWeb.Migrations
{
    public partial class AddEquipmentQuantity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Equipments");

            migrationBuilder.AddColumn<int>(
                name: "BrokenQuantity",
                table: "Equipments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaintenanceQuantity",
                table: "Equipments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalQuantity",
                table: "Equipments",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrokenQuantity",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "MaintenanceQuantity",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "TotalQuantity",
                table: "Equipments");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Equipments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
