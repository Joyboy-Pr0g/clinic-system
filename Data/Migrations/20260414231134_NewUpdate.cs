using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeNursingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class NewUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicWeeklySlots",
                columns: table => new
                {
                    ClinicWeeklySlotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicWeeklySlots", x => x.ClinicWeeklySlotId);
                    table.ForeignKey(
                        name: "FK_ClinicWeeklySlots_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "ClinicId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NurseWeeklySlots",
                columns: table => new
                {
                    NurseWeeklySlotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NurseProfileId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NurseWeeklySlots", x => x.NurseWeeklySlotId);
                    table.ForeignKey(
                        name: "FK_NurseWeeklySlots_NurseProfiles_NurseProfileId",
                        column: x => x.NurseProfileId,
                        principalTable: "NurseProfiles",
                        principalColumn: "NurseProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicWeeklySlots_ClinicId_DayOfWeek",
                table: "ClinicWeeklySlots",
                columns: new[] { "ClinicId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_NurseWeeklySlots_NurseProfileId_DayOfWeek",
                table: "NurseWeeklySlots",
                columns: new[] { "NurseProfileId", "DayOfWeek" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicWeeklySlots");

            migrationBuilder.DropTable(
                name: "NurseWeeklySlots");
        }
    }
}
