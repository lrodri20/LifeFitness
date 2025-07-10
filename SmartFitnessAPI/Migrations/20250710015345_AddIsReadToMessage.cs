using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFitness.Migrations
{
    /// <inheritdoc />
    public partial class AddIsReadToMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                schema: "auth",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRead",
                schema: "auth",
                table: "Messages");
        }
    }
}
