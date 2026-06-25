using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelManangment.Migrations
{
    /// <inheritdoc />
    public partial class AddTableNovelFollow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NovelFollow",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    NovelId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NovelFollow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NovelFollow_Novel_NovelId",
                        column: x => x.NovelId,
                        principalTable: "Novel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NovelFollow_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NovelFollow_NovelId",
                table: "NovelFollow",
                column: "NovelId");

            migrationBuilder.CreateIndex(
                name: "IX_NovelFollow_UserId_NovelId",
                table: "NovelFollow",
                columns: new[] { "UserId", "NovelId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NovelFollow");
        }
    }
}
