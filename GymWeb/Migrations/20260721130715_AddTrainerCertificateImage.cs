using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymWeb.Migrations
{
    public partial class AddTrainerCertificateImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CertificateImagePath",
                table: "TrainerProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificateImagePath",
                table: "TrainerProfiles");
        }
    }
}
