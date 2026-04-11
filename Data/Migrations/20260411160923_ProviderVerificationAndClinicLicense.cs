using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeNursingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProviderVerificationAndClinicLicense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminRejectionNote",
                table: "NurseProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRejectedByAdmin",
                table: "NurseProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AdminRejectionNote",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRejectedByAdmin",
                table: "Clinics",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LicenseDocumentPath",
                table: "Clinics",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminRejectionNote",
                table: "NurseProfiles");

            migrationBuilder.DropColumn(
                name: "IsRejectedByAdmin",
                table: "NurseProfiles");

            migrationBuilder.DropColumn(
                name: "AdminRejectionNote",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "IsRejectedByAdmin",
                table: "Clinics");

            migrationBuilder.DropColumn(
                name: "LicenseDocumentPath",
                table: "Clinics");
        }
    }
}
