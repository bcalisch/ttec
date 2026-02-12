using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoOps.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGeoOpsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "geoops");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Users",
                newSchema: "geoops");

            migrationBuilder.RenameTable(
                name: "TestTypes",
                newName: "TestTypes",
                newSchema: "geoops");

            migrationBuilder.RenameTable(
                name: "TestResults",
                newName: "TestResults",
                newSchema: "geoops");

            migrationBuilder.RenameTable(
                name: "Sensors",
                newName: "Sensors",
                newSchema: "geoops");

            migrationBuilder.RenameTable(
                name: "SensorReadings",
                newName: "SensorReadings",
                newSchema: "geoops");

            migrationBuilder.RenameTable(
                name: "Projects",
                newName: "Projects",
                newSchema: "geoops");

            migrationBuilder.RenameTable(
                name: "ProjectMemberships",
                newName: "ProjectMemberships",
                newSchema: "geoops");

            migrationBuilder.RenameTable(
                name: "ProjectBoundaries",
                newName: "ProjectBoundaries",
                newSchema: "geoops");

            migrationBuilder.RenameTable(
                name: "Observations",
                newName: "Observations",
                newSchema: "geoops");

            migrationBuilder.RenameTable(
                name: "IngestBatches",
                newName: "IngestBatches",
                newSchema: "geoops");

            migrationBuilder.RenameTable(
                name: "AuditLogs",
                newName: "AuditLogs",
                newSchema: "geoops");

            migrationBuilder.RenameTable(
                name: "Attachments",
                newName: "Attachments",
                newSchema: "geoops");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Users",
                schema: "geoops",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "TestTypes",
                schema: "geoops",
                newName: "TestTypes");

            migrationBuilder.RenameTable(
                name: "TestResults",
                schema: "geoops",
                newName: "TestResults");

            migrationBuilder.RenameTable(
                name: "Sensors",
                schema: "geoops",
                newName: "Sensors");

            migrationBuilder.RenameTable(
                name: "SensorReadings",
                schema: "geoops",
                newName: "SensorReadings");

            migrationBuilder.RenameTable(
                name: "Projects",
                schema: "geoops",
                newName: "Projects");

            migrationBuilder.RenameTable(
                name: "ProjectMemberships",
                schema: "geoops",
                newName: "ProjectMemberships");

            migrationBuilder.RenameTable(
                name: "ProjectBoundaries",
                schema: "geoops",
                newName: "ProjectBoundaries");

            migrationBuilder.RenameTable(
                name: "Observations",
                schema: "geoops",
                newName: "Observations");

            migrationBuilder.RenameTable(
                name: "IngestBatches",
                schema: "geoops",
                newName: "IngestBatches");

            migrationBuilder.RenameTable(
                name: "AuditLogs",
                schema: "geoops",
                newName: "AuditLogs");

            migrationBuilder.RenameTable(
                name: "Attachments",
                schema: "geoops",
                newName: "Attachments");
        }
    }
}
