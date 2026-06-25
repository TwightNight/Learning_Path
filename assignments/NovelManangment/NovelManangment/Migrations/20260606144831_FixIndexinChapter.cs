using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelManangment.Migrations
{
    /// <inheritdoc />
    public partial class FixIndexinChapter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chapter_NovelId_ChapterNumber",
                table: "Chapter");

            migrationBuilder.CreateIndex(
                name: "IX_Chapter_NovelId_VolumeId_ChapterNumber",
                table: "Chapter",
                columns: new[] { "NovelId", "VolumeId", "ChapterNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chapter_NovelId_VolumeId_ChapterNumber",
                table: "Chapter");

            migrationBuilder.CreateIndex(
                name: "IX_Chapter_NovelId_ChapterNumber",
                table: "Chapter",
                columns: new[] { "NovelId", "ChapterNumber" },
                unique: true);
        }
    }
}
