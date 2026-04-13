using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeNursingSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AppointmentClinicService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent for SQL Server: some databases may already have been partially migrated.
            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.columns c
    WHERE c.object_id = OBJECT_ID(N'[Appointments]') AND c.name = N'ServiceId' AND c.is_nullable = 0)
BEGIN
    DECLARE @dc sysname;
    SELECT @dc = d.name
    FROM sys.default_constraints d
    INNER JOIN sys.columns col ON col.default_object_id = d.object_id
    WHERE d.parent_object_id = OBJECT_ID(N'[Appointments]') AND col.name = N'ServiceId';
    IF @dc IS NOT NULL EXEC(N'ALTER TABLE [Appointments] DROP CONSTRAINT [' + @dc + N'];');
    ALTER TABLE [Appointments] ALTER COLUMN [ServiceId] int NULL;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Appointments', N'ClinicServiceId') IS NULL
    ALTER TABLE [Appointments] ADD [ClinicServiceId] int NULL;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes WHERE name = N'IX_Appointments_ClinicServiceId' AND object_id = OBJECT_ID(N'[Appointments]'))
    CREATE INDEX [IX_Appointments_ClinicServiceId] ON [Appointments] ([ClinicServiceId]);
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Appointments_ClinicServices_ClinicServiceId')
    ALTER TABLE [Appointments] ADD CONSTRAINT [FK_Appointments_ClinicServices_ClinicServiceId]
        FOREIGN KEY ([ClinicServiceId]) REFERENCES [ClinicServices] ([ClinicServiceId]) ON DELETE NO ACTION;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_ClinicServices_ClinicServiceId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ClinicServiceId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ClinicServiceId",
                table: "Appointments");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceId",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
