using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProDialer.Functions.Migrations
{
    /// <inheritdoc />
    public partial class AddTranscriptionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "TranscriptionConfidence",
                table: "CallLogs",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TranscriptionEnabled",
                table: "CallLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TranscriptionLanguage",
                table: "CallLogs",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TranscriptionStatus",
                table: "CallLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TranscriptionText",
                table: "CallLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TranscriptionConfidence",
                table: "CallLogs");

            migrationBuilder.DropColumn(
                name: "TranscriptionEnabled",
                table: "CallLogs");

            migrationBuilder.DropColumn(
                name: "TranscriptionLanguage",
                table: "CallLogs");

            migrationBuilder.DropColumn(
                name: "TranscriptionStatus",
                table: "CallLogs");

            migrationBuilder.DropColumn(
                name: "TranscriptionText",
                table: "CallLogs");
        }
    }
}
