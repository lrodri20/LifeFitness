using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFitness.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProfileId1",
                schema: "auth",
                table: "ProfileActivity",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProfileActivity_ProfileId1",
                schema: "auth",
                table: "ProfileActivity",
                column: "ProfileId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileActivity_Profiles_ProfileId1",
                schema: "auth",
                table: "ProfileActivity",
                column: "ProfileId1",
                principalSchema: "auth",
                principalTable: "Profiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfileActivity_Profiles_ProfileId1",
                schema: "auth",
                table: "ProfileActivity");

            migrationBuilder.DropIndex(
                name: "IX_ProfileActivity_ProfileId1",
                schema: "auth",
                table: "ProfileActivity");

            migrationBuilder.DropColumn(
                name: "ProfileId1",
                schema: "auth",
                table: "ProfileActivity");
        }
    }
}
