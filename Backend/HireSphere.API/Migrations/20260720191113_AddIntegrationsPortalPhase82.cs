using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HireSphere.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIntegrationsPortalPhase82 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationOutbox",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotificationType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DestinationMasked = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    SafeFailureCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BodySummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QueuedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationOutbox", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationOutbox_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserNotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SmsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    InterviewReminders = table.Column<bool>(type: "bit", nullable: false),
                    ApplicationUpdates = table.Column<bool>(type: "bit", nullable: false),
                    AssessmentReminders = table.Column<bool>(type: "bit", nullable: false),
                    SmsConsent = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotificationPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_IdempotencyKey",
                table: "NotificationOutbox",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_Status_QueuedAtUtc",
                table: "NotificationOutbox",
                columns: new[] { "Status", "QueuedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationOutbox_UserId",
                table: "NotificationOutbox",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreferences_UserId",
                table: "UserNotificationPreferences",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationOutbox");

            migrationBuilder.DropTable(
                name: "UserNotificationPreferences");
        }
    }
}
