using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireSphere.API.Migrations
{
    /// <inheritdoc />
    public partial class AddHiringManagerPortalPhase62 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InterviewFeedbacks_InterviewId",
                table: "InterviewFeedbacks");

            migrationBuilder.DropIndex(
                name: "IX_CandidateEvaluations_ApplicationId",
                table: "CandidateEvaluations");

            migrationBuilder.AddColumn<decimal>(
                name: "Communication",
                table: "InterviewFeedbacks",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Concerns",
                table: "InterviewFeedbacks",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CulturalContribution",
                table: "InterviewFeedbacks",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Leadership",
                table: "InterviewFeedbacks",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivatePanelComments",
                table: "InterviewFeedbacks",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProblemSolving",
                table: "InterviewFeedbacks",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Recommendation",
                table: "InterviewFeedbacks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RoleKnowledge",
                table: "InterviewFeedbacks",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Strengths",
                table: "InterviewFeedbacks",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAtUtc",
                table: "InterviewFeedbacks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Teamwork",
                table: "InterviewFeedbacks",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TechnicalCompetency",
                table: "InterviewFeedbacks",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "InterviewFeedbacks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DecisionType",
                table: "HiringDecisions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinal",
                table: "HiringDecisions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PriorApplicationStatus",
                table: "HiringDecisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "HiringDecisions",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ResultingApplicationStatus",
                table: "HiringDecisions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AssessmentPerformance",
                table: "CandidateEvaluations",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Communication",
                table: "CandidateEvaluations",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentedRisks",
                table: "CandidateEvaluations",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EducationRequirement",
                table: "CandidateEvaluations",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InterviewPerformance",
                table: "CandidateEvaluations",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Justification",
                table: "CandidateEvaluations",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PreferredSkillsAlignment",
                table: "CandidateEvaluations",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProblemSolving",
                table: "CandidateEvaluations",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RelevantExperience",
                table: "CandidateEvaluations",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RequiredSkillsAlignment",
                table: "CandidateEvaluations",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RoleReadiness",
                table: "CandidateEvaluations",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubmissionStatus",
                table: "CandidateEvaluations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAtUtc",
                table: "CandidateEvaluations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InterviewFeedbacks_InterviewId_InterviewerId",
                table: "InterviewFeedbacks",
                columns: new[] { "InterviewId", "InterviewerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEvaluations_ApplicationId_EvaluatorUserId",
                table: "CandidateEvaluations",
                columns: new[] { "ApplicationId", "EvaluatorUserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InterviewFeedbacks_InterviewId_InterviewerId",
                table: "InterviewFeedbacks");

            migrationBuilder.DropIndex(
                name: "IX_CandidateEvaluations_ApplicationId_EvaluatorUserId",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "Communication",
                table: "InterviewFeedbacks");

            migrationBuilder.DropColumn(
                name: "Concerns",
                table: "InterviewFeedbacks");

            migrationBuilder.DropColumn(
                name: "CulturalContribution",
                table: "InterviewFeedbacks");

            migrationBuilder.DropColumn(
                name: "Leadership",
                table: "InterviewFeedbacks");

            migrationBuilder.DropColumn(
                name: "PrivatePanelComments",
                table: "InterviewFeedbacks");

            migrationBuilder.DropColumn(
                name: "ProblemSolving",
                table: "InterviewFeedbacks");

            migrationBuilder.DropColumn(
                name: "Recommendation",
                table: "InterviewFeedbacks");

            migrationBuilder.DropColumn(
                name: "RoleKnowledge",
                table: "InterviewFeedbacks");

            migrationBuilder.DropColumn(
                name: "Strengths",
                table: "InterviewFeedbacks");

            migrationBuilder.DropColumn(
                name: "SubmittedAtUtc",
                table: "InterviewFeedbacks");

            migrationBuilder.DropColumn(
                name: "Teamwork",
                table: "InterviewFeedbacks");

            migrationBuilder.DropColumn(
                name: "TechnicalCompetency",
                table: "InterviewFeedbacks");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "InterviewFeedbacks");

            migrationBuilder.DropColumn(
                name: "DecisionType",
                table: "HiringDecisions");

            migrationBuilder.DropColumn(
                name: "IsFinal",
                table: "HiringDecisions");

            migrationBuilder.DropColumn(
                name: "PriorApplicationStatus",
                table: "HiringDecisions");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "HiringDecisions");

            migrationBuilder.DropColumn(
                name: "ResultingApplicationStatus",
                table: "HiringDecisions");

            migrationBuilder.DropColumn(
                name: "AssessmentPerformance",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "Communication",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "DocumentedRisks",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "EducationRequirement",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "InterviewPerformance",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "Justification",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "PreferredSkillsAlignment",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "ProblemSolving",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "RelevantExperience",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "RequiredSkillsAlignment",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "RoleReadiness",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "SubmissionStatus",
                table: "CandidateEvaluations");

            migrationBuilder.DropColumn(
                name: "SubmittedAtUtc",
                table: "CandidateEvaluations");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewFeedbacks_InterviewId",
                table: "InterviewFeedbacks",
                column: "InterviewId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidateEvaluations_ApplicationId",
                table: "CandidateEvaluations",
                column: "ApplicationId");
        }
    }
}
