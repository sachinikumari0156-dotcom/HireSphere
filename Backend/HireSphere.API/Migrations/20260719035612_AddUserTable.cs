using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireSphere.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidateProfiles_User_UserId",
                table: "CandidateProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_User_RecruiterId",
                table: "Jobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Jobs",
                table: "Jobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CandidateProfiles",
                table: "CandidateProfiles");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "user");

            migrationBuilder.RenameTable(
                name: "Jobs",
                newName: "jobs");

            migrationBuilder.RenameTable(
                name: "CandidateProfiles",
                newName: "candidateprofiles");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_RecruiterId",
                table: "jobs",
                newName: "IX_jobs_RecruiterId");

            migrationBuilder.RenameIndex(
                name: "IX_CandidateProfiles_UserId",
                table: "candidateprofiles",
                newName: "IX_candidateprofiles_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user",
                table: "user",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_jobs",
                table: "jobs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_candidateprofiles",
                table: "candidateprofiles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_candidateprofiles_user_UserId",
                table: "candidateprofiles",
                column: "UserId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_jobs_user_RecruiterId",
                table: "jobs",
                column: "RecruiterId",
                principalTable: "user",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_candidateprofiles_user_UserId",
                table: "candidateprofiles");

            migrationBuilder.DropForeignKey(
                name: "FK_jobs_user_RecruiterId",
                table: "jobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user",
                table: "user");

            migrationBuilder.DropPrimaryKey(
                name: "PK_jobs",
                table: "jobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_candidateprofiles",
                table: "candidateprofiles");

            migrationBuilder.RenameTable(
                name: "user",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "jobs",
                newName: "Jobs");

            migrationBuilder.RenameTable(
                name: "candidateprofiles",
                newName: "CandidateProfiles");

            migrationBuilder.RenameIndex(
                name: "IX_jobs_RecruiterId",
                table: "Jobs",
                newName: "IX_Jobs_RecruiterId");

            migrationBuilder.RenameIndex(
                name: "IX_candidateprofiles_UserId",
                table: "CandidateProfiles",
                newName: "IX_CandidateProfiles_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Jobs",
                table: "Jobs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CandidateProfiles",
                table: "CandidateProfiles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateProfiles_User_UserId",
                table: "CandidateProfiles",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_User_RecruiterId",
                table: "Jobs",
                column: "RecruiterId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
