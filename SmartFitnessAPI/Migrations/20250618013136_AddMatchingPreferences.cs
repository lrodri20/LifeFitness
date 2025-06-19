using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFitness.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchingPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FitnessLevel",
                schema: "auth",
                table: "Profiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasHomeGym",
                schema: "auth",
                table: "Profiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                schema: "auth",
                table: "Profiles",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                schema: "auth",
                table: "Profiles",
                type: "float",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Activities",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IconUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
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

            migrationBuilder.CreateTable(
                name: "MatchingPreferences",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    MaxDistanceMiles = table.Column<int>(type: "int", nullable: false),
                    MinAge = table.Column<int>(type: "int", nullable: true),
                    MaxAge = table.Column<int>(type: "int", nullable: true),
                    GenderPreference = table.Column<int>(type: "int", nullable: false),
                    PreferSimilarFitnessLevel = table.Column<bool>(type: "bit", nullable: false),
                    FitnessLevelTolerance = table.Column<int>(type: "int", nullable: false),
                    PreferHomeGym = table.Column<bool>(type: "bit", nullable: false),
                    PreferPublicGym = table.Column<bool>(type: "bit", nullable: false),
                    PreferOutdoor = table.Column<bool>(type: "bit", nullable: false),
                    OpenToGroupWorkouts = table.Column<bool>(type: "bit", nullable: false),
                    MaxGroupSize = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchingPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchingPreferences_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "auth",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileGoal",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    Goal = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileGoal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileGoal_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "auth",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileSchedule",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    TimeSlot = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileSchedule_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "auth",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfileActivity",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileActivity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileActivity_Activities_ActivityId",
                        column: x => x.ActivityId,
                        principalSchema: "auth",
                        principalTable: "Activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProfileActivity_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "auth",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "IX_MatchingPreferences_ProfileId",
                schema: "auth",
                table: "MatchingPreferences",
                column: "ProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileActivity_ActivityId",
                schema: "auth",
                table: "ProfileActivity",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileActivity_ProfileId",
                schema: "auth",
                table: "ProfileActivity",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileGoal_ProfileId",
                schema: "auth",
                table: "ProfileGoal",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileSchedule_ProfileId",
                schema: "auth",
                table: "ProfileSchedule",
                column: "ProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matches",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "MatchingPreferences",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "ProfileActivity",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "ProfileGoal",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "ProfileSchedule",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Activities",
                schema: "auth");

            migrationBuilder.DropColumn(
                name: "FitnessLevel",
                schema: "auth",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "HasHomeGym",
                schema: "auth",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Latitude",
                schema: "auth",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Longitude",
                schema: "auth",
                table: "Profiles");
        }
    }
}
