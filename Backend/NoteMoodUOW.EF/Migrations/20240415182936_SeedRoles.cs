using Microsoft.EntityFrameworkCore.Migrations;
using NoteMoodUOW.Core.Constants;

#nullable disable

namespace NoteMoodUOW.EF.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "Name", "NormalizedName", "ConcurrencyStamp" },
                values: new object[] { Guid.NewGuid().ToString(), Roles.DiaryUser.ToString(), $"{Roles.DiaryUser}".ToUpper(), Guid.NewGuid().ToString() });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Name",
                keyValue: Roles.DiaryUser.ToString());
        }
    }
}
