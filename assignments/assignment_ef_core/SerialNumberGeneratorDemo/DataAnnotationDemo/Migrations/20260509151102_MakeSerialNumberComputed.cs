using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAnnotationDemo.Migrations
{
    /// <inheritdoc />
    public partial class MakeSerialNumberComputed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                computedColumnSql:
                    "CAST([Id] AS VARCHAR) + '-' + CONVERT(VARCHAR(8), [CreatedDate], 112) + '-' + CAST([Id] AS VARCHAR)",
                stored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SerialNumber",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "SerialNumber",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
