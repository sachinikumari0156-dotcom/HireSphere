using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireSphere.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruiterPortalPhase52 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "SkillAssessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "SkillAssessments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApplicationMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    SenderUserId = table.Column<int>(type: "int", nullable: false),
                    SenderRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsReadByRecipient = table.Column<bool>(type: "bit", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationMessages_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationMessages_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RankingReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    ReviewerUserId = table.Column<int>(type: "int", nullable: false),
                    OverrideScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Decision = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankingReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RankingReviews_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RankingReviews_Users_ReviewerUserId",
                        column: x => x.ReviewerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkillAssessments_OrganizationId",
                table: "SkillAssessments",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationMessages_ApplicationId_SentAtUtc",
                table: "ApplicationMessages",
                columns: new[] { "ApplicationId", "SentAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationMessages_SenderUserId",
                table: "ApplicationMessages",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RankingReviews_ApplicationId",
                table: "RankingReviews",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_RankingReviews_ReviewerUserId",
                table: "RankingReviews",
                column: "ReviewerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SkillAssessments_Organizations_OrganizationId",
                table: "SkillAssessments",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SkillAssessments_Organizations_OrganizationId",
                table: "SkillAssessments");

            migrationBuilder.DropTable(
                name: "ApplicationMessages");

            migrationBuilder.DropTable(
                name: "RankingReviews");

            migrationBuilder.DropIndex(
                name: "IX_SkillAssessments_OrganizationId",
                table: "SkillAssessments");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "SkillAssessments");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "SkillAssessments");
        }
    }
}
