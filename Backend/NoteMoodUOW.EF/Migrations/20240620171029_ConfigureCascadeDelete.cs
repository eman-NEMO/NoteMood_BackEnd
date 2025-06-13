using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoteMoodUOW.EF.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_AspNetUsers_ApplicationUserId",
                table: "Entries");

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_AspNetUsers_ApplicationUserId",
                table: "Entries",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entries_AspNetUsers_ApplicationUserId",
                table: "Entries");

            migrationBuilder.AddForeignKey(
                name: "FK_Entries_AspNetUsers_ApplicationUserId",
                table: "Entries",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
