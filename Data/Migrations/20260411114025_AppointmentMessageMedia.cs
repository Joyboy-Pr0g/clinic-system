using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeNursingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AppointmentMessageMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "AppointmentMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MessageType",
                table: "AppointmentMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "AppointmentMessages");

            migrationBuilder.DropColumn(
                name: "MessageType",
                table: "AppointmentMessages");
        }
    }
}
