using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InteractiveStory.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordHint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHint",
                table: "Stories",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Stories",
                keyColumn: "Id",
                keyValue: new Guid("e819b5c2-f170-4eb6-9280-928570e30d12"),
                column: "PasswordHint",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHint",
                table: "Stories");
        }
    }
}
