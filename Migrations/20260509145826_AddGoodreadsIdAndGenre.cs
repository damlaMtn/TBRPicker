using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TBRPicker.Migrations
{
    /// <inheritdoc />
    public partial class AddGoodreadsIdAndGenre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoodreadsId",
                table: "Books",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoodreadsId",
                table: "Books");
        }
    }
}
