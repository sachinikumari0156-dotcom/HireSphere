using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireSphere.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAiPortalPhase81 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ConsentUsedExternal",
                table: "ResumeAnalyses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedYearsExperience",
                table: "ResumeAnalyses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractedEmail",
                table: "ResumeAnalyses",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractedName",
                table: "ResumeAnalyses",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractedPhone",
                table: "ResumeAnalyses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractedSummary",
                table: "ResumeAnalyses",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "ResumeAnalyses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FallbackNote",
                table: "ResumeAnalyses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GeneratedAtUtc",
                table: "ResumeAnalyses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "ResumeAnalyses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProviderType",
                table: "ResumeAnalyses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProviderVersion",
                table: "ResumeAnalyses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ResumeAnalyses",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "ResumeAnalyses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowExternalAiProcessing",
                table: "CandidateProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExternalAiConsentAtUtc",
                table: "CandidateProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExtractedSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResumeAnalysisId = table.Column<int>(type: "int", nullable: false),
                    RawName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CanonicalName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Confidence = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SourceEvidence = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractedSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtractedSkills_ResumeAnalyses_ResumeAnalysisId",
                        column: x => x.ResumeAnalysisId,
                        principalTable: "ResumeAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExtractedSkills_ResumeAnalysisId_CanonicalName",
                table: "ExtractedSkills",
                columns: new[] { "ResumeAnalysisId", "CanonicalName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExtractedSkills");

            migrationBuilder.DropColumn(
                name: "ConsentUsedExternal",
                table: "ResumeAnalyses");

            migrationBuilder.DropColumn(
                name: "EstimatedYearsExperience",
                table: "ResumeAnalyses");

            migrationBuilder.DropColumn(
                name: "ExtractedEmail",
                table: "ResumeAnalyses");

            migrationBuilder.DropColumn(
                name: "ExtractedName",
                table: "ResumeAnalyses");

            migrationBuilder.DropColumn(
                name: "ExtractedPhone",
                table: "ResumeAnalyses");

            migrationBuilder.DropColumn(
                name: "ExtractedSummary",
                table: "ResumeAnalyses");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "ResumeAnalyses");

            migrationBuilder.DropColumn(
                name: "FallbackNote",
                table: "ResumeAnalyses");

            migrationBuilder.DropColumn(
                name: "GeneratedAtUtc",
                table: "ResumeAnalyses");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "ResumeAnalyses");

            migrationBuilder.DropColumn(
                name: "ProviderType",
                table: "ResumeAnalyses");

            migrationBuilder.DropColumn(
                name: "ProviderVersion",
                table: "ResumeAnalyses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ResumeAnalyses");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ResumeAnalyses");

            migrationBuilder.DropColumn(
                name: "AllowExternalAiProcessing",
                table: "CandidateProfiles");

            migrationBuilder.DropColumn(
                name: "ExternalAiConsentAtUtc",
                table: "CandidateProfiles");
        }
    }
}
