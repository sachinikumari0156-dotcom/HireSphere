using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireSphere.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPhase43AssessmentsInterviewsNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications");

            migrationBuilder.AddColumn<int>(
                name: "MaxAttempts",
                table: "SkillAssessments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PassingScorePercent",
                table: "SkillAssessments",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "RevealResultsToCandidate",
                table: "SkillAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Notifications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LinkPath",
                table: "Notifications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RelatedEntityId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RelatedEntityType",
                table: "Notifications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CandidateRespondedAtUtc",
                table: "Interviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CandidateResponse",
                table: "Interviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CandidateResponseReason",
                table: "Interviews",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingInstructions",
                table: "Interviews",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireConfirmForMeetingInfo",
                table: "Interviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "Interviews",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Interviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrectAnswerKey",
                table: "AssessmentQuestions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OptionsJson",
                table: "AssessmentQuestions",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssessmentAssignmentId",
                table: "AssessmentAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "AttemptExpiresAtUtc",
                table: "AssessmentAttempts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssessmentAnswers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentAttemptId = table.Column<int>(type: "int", nullable: false),
                    AssessmentQuestionId = table.Column<int>(type: "int", nullable: false),
                    AnswerValue = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    AwardedPoints = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentAnswers_AssessmentAttempts_AssessmentAttemptId",
                        column: x => x.AssessmentAttemptId,
                        principalTable: "AssessmentAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentAnswers_AssessmentQuestions_AssessmentQuestionId",
                        column: x => x.AssessmentQuestionId,
                        principalTable: "AssessmentQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkillAssessmentId = table.Column<int>(type: "int", nullable: false),
                    CandidateId = table.Column<int>(type: "int", nullable: false),
                    ApplicationId = table.Column<int>(type: "int", nullable: true),
                    AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false),
                    RevealResultsToCandidate = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentAssignments_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AssessmentAssignments_SkillAssessments_SkillAssessmentId",
                        column: x => x.SkillAssessmentId,
                        principalTable: "SkillAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssessmentAssignments_Users_CandidateId",
                        column: x => x.CandidateId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAtUtc",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttempts_AssessmentAssignmentId",
                table: "AssessmentAttempts",
                column: "AssessmentAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAnswers_AssessmentAttemptId_AssessmentQuestionId",
                table: "AssessmentAnswers",
                columns: new[] { "AssessmentAttemptId", "AssessmentQuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAnswers_AssessmentQuestionId",
                table: "AssessmentAnswers",
                column: "AssessmentQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAssignments_ApplicationId",
                table: "AssessmentAssignments",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAssignments_CandidateId_SkillAssessmentId",
                table: "AssessmentAssignments",
                columns: new[] { "CandidateId", "SkillAssessmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAssignments_SkillAssessmentId",
                table: "AssessmentAssignments",
                column: "SkillAssessmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentAttempts_AssessmentAssignments_AssessmentAssignmentId",
                table: "AssessmentAttempts",
                column: "AssessmentAssignmentId",
                principalTable: "AssessmentAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssessmentAttempts_AssessmentAssignments_AssessmentAssignmentId",
                table: "AssessmentAttempts");

            migrationBuilder.DropTable(
                name: "AssessmentAnswers");

            migrationBuilder.DropTable(
                name: "AssessmentAssignments");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAtUtc",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_AssessmentAttempts_AssessmentAssignmentId",
                table: "AssessmentAttempts");

            migrationBuilder.DropColumn(
                name: "MaxAttempts",
                table: "SkillAssessments");

            migrationBuilder.DropColumn(
                name: "PassingScorePercent",
                table: "SkillAssessments");

            migrationBuilder.DropColumn(
                name: "RevealResultsToCandidate",
                table: "SkillAssessments");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "LinkPath",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RelatedEntityId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RelatedEntityType",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CandidateRespondedAtUtc",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "CandidateResponse",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "CandidateResponseReason",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "MeetingInstructions",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "RequireConfirmForMeetingInfo",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "CorrectAnswerKey",
                table: "AssessmentQuestions");

            migrationBuilder.DropColumn(
                name: "OptionsJson",
                table: "AssessmentQuestions");

            migrationBuilder.DropColumn(
                name: "AssessmentAssignmentId",
                table: "AssessmentAttempts");

            migrationBuilder.DropColumn(
                name: "AttemptExpiresAtUtc",
                table: "AssessmentAttempts");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");
        }
    }
}
