using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFitness.Migrations
{
    /// <inheritdoc />
    public partial class AddRevokedTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RevokedTokens",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JwtId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevokedTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RevokedTokens_JwtId",
                schema: "auth",
                table: "RevokedTokens",
                column: "JwtId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RevokedTokens",
                schema: "auth");
        }
    }
}
