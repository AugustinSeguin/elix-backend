using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElixBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_image_category_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageMediaPath",
                table: "Categories",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageMediaPath",
                table: "Categories");
        }
    }
}
