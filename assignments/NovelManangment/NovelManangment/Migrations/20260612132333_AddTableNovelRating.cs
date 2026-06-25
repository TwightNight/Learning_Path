using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelManangment.Migrations
{
    /// <inheritdoc />
    public partial class AddTableNovelRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NovelRating",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    NovelId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NovelRating", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NovelRating_Novel_NovelId",
                        column: x => x.NovelId,
                        principalTable: "Novel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NovelRating_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NovelRating_NovelId",
                table: "NovelRating",
                column: "NovelId");

            migrationBuilder.CreateIndex(
                name: "IX_NovelRating_UserId_NovelId",
                table: "NovelRating",
                columns: new[] { "UserId", "NovelId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NovelRating");
        }
    }
}
