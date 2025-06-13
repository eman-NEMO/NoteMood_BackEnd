using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteMoodUOW.EF.Migrations
{
    /// <inheritdoc />
    public partial class addRelationshipBetweenUserAndSentiment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailySentiments",
                columns: table => new
                {
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Sentiment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Percentage = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySentiments", x => new { x.ApplicationUserId, x.Date });
                    table.ForeignKey(
                        name: "FK_DailySentiments_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailySentiments");
        }
    }
}
