using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InteractiveStory.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Json = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    MaxPlaythroughs = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Playthroughs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CurrentNodeId = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playthroughs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playthroughs_Stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Stories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlaythroughHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlaythroughId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NodeId = table.Column<string>(type: "TEXT", nullable: false),
                    ChosenChoiceId = table.Column<string>(type: "TEXT", nullable: true),
                    SequenceNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaythroughHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaythroughHistories_Playthroughs_PlaythroughId",
                        column: x => x.PlaythroughId,
                        principalTable: "Playthroughs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Stories",
                columns: new[] { "Id", "CreatedAt", "Description", "Json", "MaxPlaythroughs", "Password", "Title", "UpdatedAt" },
                values: new object[] { new Guid("e819b5c2-f170-4eb6-9280-928570e30d12"), new DateTime(2026, 9, 9, 11, 36, 30, 0, DateTimeKind.Unspecified), "Test <b>story</b> \n", "{\"startNode\":\"start\",\"nodes\":{\"start\":{\"text\":\"New <b>Story</b>\",\"isEnding\":false,\"media\":[],\"choices\":[{\"id\":\"c-7dwemcw\",\"text\":\"Go up\",\"next\":\"up\",\"media\":[]},{\"id\":\"c-ymjrgmy\",\"text\":\"Go down\",\"next\":\"down\",\"media\":[]},{\"id\":\"c-wfz80t0\",\"text\":\"Go left\",\"next\":\"Left\",\"media\":[]},{\"id\":\"c-hatj1zf\",\"text\":\"Go right\",\"next\":\"right\",\"media\":[]}]},\"Left\":{\"text\":\"Left room\",\"isEnding\":false,\"media\":[],\"choices\":[{\"id\":\"c-a5li9pt\",\"text\":\"UP\",\"next\":\"UL-END\",\"media\":[]},{\"id\":\"c-eomy6ee\",\"text\":\"DOWN\",\"next\":\"DL\",\"media\":[]},{\"id\":\"c-sssocou\",\"text\":\"Right\",\"next\":\"start\",\"media\":[]}]},\"right\":{\"text\":\"Right room\",\"isEnding\":false,\"media\":[],\"choices\":[{\"id\":\"c-ptauk0f\",\"text\":\"<span style=\\\"color: red\\\">UP</span>\",\"next\":\"UR\",\"media\":[]},{\"id\":\"c-ch8c9ts\",\"text\":\"DOWN\",\"next\":\"DR\",\"media\":[]},{\"id\":\"c-ef9mtv7\",\"text\":\"LEFT\",\"next\":\"start\",\"media\":[]}]},\"up\":{\"text\":\"Up room\",\"isEnding\":false,\"media\":[],\"choices\":[{\"id\":\"c-h5vyuxr\",\"text\":\"Left\",\"next\":\"UL-END\",\"media\":[]},{\"id\":\"c-4aqymv3\",\"text\":\"Right\",\"next\":\"UR\",\"media\":[]},{\"id\":\"c-a5xl0pl\",\"text\":\"Down\",\"next\":\"start\",\"media\":[]}]},\"down\":{\"text\":\"Down room\",\"isEnding\":false,\"media\":[],\"choices\":[{\"id\":\"c-2gjqpn1\",\"text\":\"Left\",\"next\":\"DL\",\"media\":[]},{\"id\":\"c-mj6bp7g\",\"text\":\"Right\",\"next\":\"DR\",\"media\":[]},{\"id\":\"c-vovbuug\",\"text\":\"Up\",\"next\":\"start\",\"media\":[]}]},\"UL-END\":{\"text\":\"Up Left Room - The END\",\"isEnding\":true,\"media\":[],\"choices\":[]},\"DL\":{\"text\":\"Down Left Room\",\"isEnding\":false,\"media\":[],\"choices\":[{\"id\":\"c-wi1jeq3\",\"text\":\"Up\",\"next\":\"Left\",\"media\":[]},{\"id\":\"c-f9ww0vy\",\"text\":\"Right\",\"next\":\"down\",\"media\":[]}]},\"UR\":{\"text\":\"Up Right Room\",\"isEnding\":false,\"media\":[],\"choices\":[{\"id\":\"c-wo0sjkg\",\"text\":\"Left\",\"next\":\"up\",\"media\":[]},{\"id\":\"c-hcti6wp\",\"text\":\"Down\",\"next\":\"right\",\"media\":[]}]},\"DR\":{\"text\":\"Down Right Room\",\"isEnding\":false,\"media\":[],\"choices\":[{\"id\":\"c-nxncv8s\",\"text\":\"Up\",\"next\":\"right\",\"media\":[]},{\"id\":\"c-kdqenhp\",\"text\":\"Left\",\"next\":\"down\",\"media\":[]}]}}}", 2, "pass", "Test", new DateTime(2026, 9, 10, 7, 48, 46, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Password", "Username" },
                values: new object[] { new Guid("4c05e191-2c9e-4eb9-a7e1-881c15b14441"), "admin", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_PlaythroughHistories_PlaythroughId",
                table: "PlaythroughHistories",
                column: "PlaythroughId");

            migrationBuilder.CreateIndex(
                name: "IX_Playthroughs_StoryId",
                table: "Playthroughs",
                column: "StoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaythroughHistories");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Playthroughs");

            migrationBuilder.DropTable(
                name: "Stories");
        }
    }
}
