using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteMoodUOW.EF.Migrations
{
    /// <inheritdoc />
    public partial class addRelationBetweenEntryAndSentiment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OverallSentiment",
                table: "Entries");

            migrationBuilder.AddColumn<int>(
                name: "SentimentId",
                table: "Entries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Entries_SentimentId",
                table: "Entries",
                column: "SentimentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_Sentiment_SentimentId",
                table: "Entries",
                column: "SentimentId",
                principalTable: "Sentiment",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetDefault);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_Sentiment_SentimentId",
                table: "Entries");

            migrationBuilder.DropIndex(
                name: "IX_Entries_SentimentId",
                table: "Entries");

            migrationBuilder.DropColumn(
                name: "SentimentId",
                table: "Entries");

            migrationBuilder.AddColumn<string>(
                name: "OverallSentiment",
                table: "Entries",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
