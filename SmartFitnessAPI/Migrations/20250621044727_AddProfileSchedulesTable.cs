using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFitness.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileSchedulesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfileSchedule_Profiles_ProfileId",
                schema: "auth",
                table: "ProfileSchedule");

            migrationBuilder.DropTable(
                name: "Matches",
                schema: "auth");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfileSchedule",
                schema: "auth",
                table: "ProfileSchedule");

            migrationBuilder.RenameTable(
                name: "ProfileSchedule",
                schema: "auth",
                newName: "ProfileSchedules",
                newSchema: "auth");

            migrationBuilder.RenameIndex(
                name: "IX_ProfileSchedule_ProfileId",
                schema: "auth",
                table: "ProfileSchedules",
                newName: "IX_ProfileSchedules_ProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfileSchedules",
                schema: "auth",
                table: "ProfileSchedules",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "MatchRequests",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequesterId = table.Column<int>(type: "int", nullable: false),
                    RequesteeId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompatibilityScore = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastInteractionAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InitialMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SharedActivitiesJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchRequests_Profiles_RequesteeId",
                        column: x => x.RequesteeId,
                        principalSchema: "auth",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatchRequests_Profiles_RequesterId",
                        column: x => x.RequesterId,
                        principalSchema: "auth",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MatchRequests_RequesteeId",
                schema: "auth",
                table: "MatchRequests",
                column: "RequesteeId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchRequests_RequesterId",
                schema: "auth",
                table: "MatchRequests",
                column: "RequesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileSchedules_Profiles_ProfileId",
                schema: "auth",
                table: "ProfileSchedules",
                column: "ProfileId",
                principalSchema: "auth",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfileSchedules_Profiles_ProfileId",
                schema: "auth",
                table: "ProfileSchedules");

            migrationBuilder.DropTable(
                name: "MatchRequests",
                schema: "auth");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfileSchedules",
                schema: "auth",
                table: "ProfileSchedules");

            migrationBuilder.RenameTable(
                name: "ProfileSchedules",
                schema: "auth",
                newName: "ProfileSchedule",
                newSchema: "auth");

            migrationBuilder.RenameIndex(
                name: "IX_ProfileSchedules_ProfileId",
                schema: "auth",
                table: "ProfileSchedule",
                newName: "IX_ProfileSchedule_ProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfileSchedule",
                schema: "auth",
                table: "ProfileSchedule",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Matches",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequesteeId = table.Column<int>(type: "int", nullable: false),
                    RequesterId = table.Column<int>(type: "int", nullable: false),
                    CompatibilityScore = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InitialMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastInteractionAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SharedActivitiesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Profiles_RequesteeId",
                        column: x => x.RequesteeId,
                        principalSchema: "auth",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Profiles_RequesterId",
                        column: x => x.RequesterId,
                        principalSchema: "auth",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_RequesteeId",
                schema: "auth",
                table: "Matches",
                column: "RequesteeId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_RequesterId",
                schema: "auth",
                table: "Matches",
                column: "RequesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileSchedule_Profiles_ProfileId",
                schema: "auth",
                table: "ProfileSchedule",
                column: "ProfileId",
                principalSchema: "auth",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
