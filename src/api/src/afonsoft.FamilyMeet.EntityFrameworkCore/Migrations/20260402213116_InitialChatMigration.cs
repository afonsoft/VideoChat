using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace afonsoft.FamilyMeet.Migrations
{
    /// <inheritdoc />
    public partial class InitialChatMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppChatChatGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    MaxParticipants = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppChatChatGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppChatChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsEdited = table.Column<bool>(type: "boolean", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ReplyToMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppChatChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppChatChatMessages_AppChatChatGroups_ChatGroupId",
                        column: x => x.ChatGroupId,
                        principalTable: "AppChatChatGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppChatChatMessages_AppChatChatMessages_ReplyToMessageId",
                        column: x => x.ReplyToMessageId,
                        principalTable: "AppChatChatMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppChatChatParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsOnline = table.Column<bool>(type: "boolean", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsMuted = table.Column<bool>(type: "boolean", nullable: false),
                    IsBanned = table.Column<bool>(type: "boolean", nullable: false),
                    BannedUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsCreator = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppChatChatParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppChatChatParticipants_AppChatChatGroups_ChatGroupId",
                        column: x => x.ChatGroupId,
                        principalTable: "AppChatChatGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatGroups_CreationTime",
                table: "AppChatChatGroups",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatGroups_IsActive",
                table: "AppChatChatGroups",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatGroups_IsPublic",
                table: "AppChatChatGroups",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatGroups_LastMessageAt",
                table: "AppChatChatGroups",
                column: "LastMessageAt");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatGroups_Name",
                table: "AppChatChatGroups",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatMessages_ChatGroupId",
                table: "AppChatChatMessages",
                column: "ChatGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatMessages_ChatGroupId_CreationTime",
                table: "AppChatChatMessages",
                columns: new[] { "ChatGroupId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatMessages_ChatGroupId_IsDeleted_CreationTime",
                table: "AppChatChatMessages",
                columns: new[] { "ChatGroupId", "IsDeleted", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatMessages_CreationTime",
                table: "AppChatChatMessages",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatMessages_IsDeleted",
                table: "AppChatChatMessages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatMessages_ReplyToMessageId",
                table: "AppChatChatMessages",
                column: "ReplyToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatMessages_SenderId",
                table: "AppChatChatMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatMessages_Type",
                table: "AppChatChatMessages",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatParticipants_ChatGroupId",
                table: "AppChatChatParticipants",
                column: "ChatGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatParticipants_ChatGroupId_UserId",
                table: "AppChatChatParticipants",
                columns: new[] { "ChatGroupId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatParticipants_IsBanned",
                table: "AppChatChatParticipants",
                column: "IsBanned");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatParticipants_IsOnline",
                table: "AppChatChatParticipants",
                column: "IsOnline");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatChatParticipants_UserId",
                table: "AppChatChatParticipants",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppChatChatMessages");

            migrationBuilder.DropTable(
                name: "AppChatChatParticipants");

            migrationBuilder.DropTable(
                name: "AppChatChatGroups");
        }
    }
}
