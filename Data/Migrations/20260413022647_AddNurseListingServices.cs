using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeNursingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNurseListingServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NurseListingServiceId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NurseListingServices",
                columns: table => new
                {
                    NurseListingServiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NurseProfileId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NurseListingServices", x => x.NurseListingServiceId);
                    table.ForeignKey(
                        name: "FK_NurseListingServices_NurseProfiles_NurseProfileId",
                        column: x => x.NurseProfileId,
                        principalTable: "NurseProfiles",
                        principalColumn: "NurseProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_NurseListingServiceId",
                table: "Appointments",
                column: "NurseListingServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_NurseListingServices_NurseProfileId",
                table: "NurseListingServices",
                column: "NurseProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_NurseListingServices_NurseListingServiceId",
                table: "Appointments",
                column: "NurseListingServiceId",
                principalTable: "NurseListingServices",
                principalColumn: "NurseListingServiceId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_NurseListingServices_NurseListingServiceId",
                table: "Appointments");

            migrationBuilder.DropTable(
                name: "NurseListingServices");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_NurseListingServiceId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "NurseListingServiceId",
                table: "Appointments");
        }
    }
}
