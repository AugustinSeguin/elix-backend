using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElixBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeQuestionToQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TypeQuestion",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeQuestion",
                table: "Questions");
        }
    }
}
