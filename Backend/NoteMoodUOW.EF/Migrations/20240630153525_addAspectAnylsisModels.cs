using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteMoodUOW.EF.Migrations
{
    /// <inheritdoc />
    public partial class addAspectAnylsisModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aspect",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aspect", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sentiment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sentiment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Entity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AspectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entity_Aspect_AspectId",
                        column: x => x.AspectId,
                        principalTable: "Aspect",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntitySentiment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    SentimentId = table.Column<int>(type: "int", nullable: false),
                    EntryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntitySentiment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntitySentiment_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntitySentiment_Entries_EntryId",
                        column: x => x.EntryId,
                        principalTable: "Entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntitySentiment_Sentiment_SentimentId",
                        column: x => x.SentimentId,
                        principalTable: "Sentiment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entity_AspectId",
                table: "Entity",
                column: "AspectId");

            migrationBuilder.CreateIndex(
                name: "IX_EntitySentiment_EntityId",
                table: "EntitySentiment",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntitySentiment_EntryId",
                table: "EntitySentiment",
                column: "EntryId");

            migrationBuilder.CreateIndex(
                name: "IX_EntitySentiment_SentimentId",
                table: "EntitySentiment",
                column: "SentimentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntitySentiment");

            migrationBuilder.DropTable(
                name: "Entity");

            migrationBuilder.DropTable(
                name: "Sentiment");

            migrationBuilder.DropTable(
                name: "Aspect");
        }
    }
}
