using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireSphere.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruiterAccessRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecruiterAccessRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BusinessEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedBusinessEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    OrganizationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecruiterAccessRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecruiterAccessRequests_Users_CreatedUserId",
                        column: x => x.CreatedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecruiterAccessRequests_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecruiterAccessRequests_CreatedUserId",
                table: "RecruiterAccessRequests",
                column: "CreatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruiterAccessRequests_NormalizedBusinessEmail",
                table: "RecruiterAccessRequests",
                column: "NormalizedBusinessEmail");

            migrationBuilder.CreateIndex(
                name: "IX_RecruiterAccessRequests_ReviewedByUserId",
                table: "RecruiterAccessRequests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruiterAccessRequests_Status",
                table: "RecruiterAccessRequests",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecruiterAccessRequests");
        }
    }
}
