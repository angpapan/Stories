using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InteractiveStory.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddStoryLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxPlaythroughs",
                table: "Stories",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Stories",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPlaythroughs",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Stories");
        }
    }
}
