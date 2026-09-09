using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InteractiveStory.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStoryDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Stories",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Stories");
        }
    }
}
