using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeNursingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChatAndLiveLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "LastLatitude",
                table: "NurseProfiles",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LastLongitude",
                table: "NurseProfiles",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LocationUpdatedAt",
                table: "NurseProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLiveLocationAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppointmentMessages",
                columns: table => new
                {
                    AppointmentMessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppointmentId = table.Column<int>(type: "int", nullable: false),
                    SenderUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentMessages", x => x.AppointmentMessageId);
                    table.ForeignKey(
                        name: "FK_AppointmentMessages_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "AppointmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppointmentMessages_AspNetUsers_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentMessages_AppointmentId_CreatedAt",
                table: "AppointmentMessages",
                columns: new[] { "AppointmentId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentMessages_SenderUserId",
                table: "AppointmentMessages",
                column: "SenderUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentMessages");

            migrationBuilder.DropColumn(
                name: "LastLatitude",
                table: "NurseProfiles");

            migrationBuilder.DropColumn(
                name: "LastLongitude",
                table: "NurseProfiles");

            migrationBuilder.DropColumn(
                name: "LocationUpdatedAt",
                table: "NurseProfiles");

            migrationBuilder.DropColumn(
                name: "LastLiveLocationAt",
                table: "AspNetUsers");
        }
    }
}
