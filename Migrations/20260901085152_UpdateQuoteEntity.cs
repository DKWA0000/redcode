using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuoteEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_quotes_books_bookId",
                table: "quotes");

            migrationBuilder.DropIndex(
                name: "IX_quotes_bookId",
                table: "quotes");

            migrationBuilder.DropColumn(
                name: "bookId",
                table: "quotes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "bookId",
                table: "quotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_quotes_bookId",
                table: "quotes",
                column: "bookId");

            migrationBuilder.AddForeignKey(
                name: "FK_quotes_books_bookId",
                table: "quotes",
                column: "bookId",
                principalTable: "books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
