using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NovelManangment.Migrations
{
    /// <inheritdoc />
    public partial class AddColunmPublishStatusInTableNovel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublishStatus",
                table: "Novel",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "Draft");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishStatus",
                table: "Novel");
        }
    }
}
