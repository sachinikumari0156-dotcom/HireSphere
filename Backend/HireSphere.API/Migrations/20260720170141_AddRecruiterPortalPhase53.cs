using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireSphere.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruiterPortalPhase53 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CalendarSyncStatus",
                table: "Interviews",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Interviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HiringManagerUserId",
                table: "Interviews",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternalNotes",
                table: "Interviews",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhysicalLocation",
                table: "Interviews",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecruiterUserId",
                table: "Interviews",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_HiringManagerUserId",
                table: "Interviews",
                column: "HiringManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_RecruiterUserId",
                table: "Interviews",
                column: "RecruiterUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Interviews_Users_HiringManagerUserId",
                table: "Interviews",
                column: "HiringManagerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Interviews_Users_RecruiterUserId",
                table: "Interviews",
                column: "RecruiterUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interviews_Users_HiringManagerUserId",
                table: "Interviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Interviews_Users_RecruiterUserId",
                table: "Interviews");

            migrationBuilder.DropIndex(
                name: "IX_Interviews_HiringManagerUserId",
                table: "Interviews");

            migrationBuilder.DropIndex(
                name: "IX_Interviews_RecruiterUserId",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "CalendarSyncStatus",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "HiringManagerUserId",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "InternalNotes",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "PhysicalLocation",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "RecruiterUserId",
                table: "Interviews");
        }
    }
}
