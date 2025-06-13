using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteMoodUOW.EF.Migrations
{
    /// <inheritdoc />
    public partial class OverAllSentimentProp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OverAllSentiment",
                table: "Entries",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverAllSentiment",
                table: "Entries");
        }
    }
}
