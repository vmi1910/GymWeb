using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymWeb.Migrations
{
    public partial class AddPackageImageAndHighlights : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BadgeText",
                table: "MembershipPackages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Highlights",
                table: "MembershipPackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "MembershipPackages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "MembershipPackages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BadgeText",
                table: "MembershipPackages");

            migrationBuilder.DropColumn(
                name: "Highlights",
                table: "MembershipPackages");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "MembershipPackages");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "MembershipPackages");
        }
    }
}
